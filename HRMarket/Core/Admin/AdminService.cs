using HRMarket.Configuration.Exceptions;
using HRMarket.Configuration.Moderation;
using HRMarket.Configuration.Redis;
using HRMarket.Configuration.Status;
using HRMarket.Configuration.Types;
using HRMarket.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Admin;

public interface IAdminService
{
    Task<List<FirmReviewListDto>> GetFirmsAwaitingReviewAsync(int page = 1, int pageSize = 20);
    Task<FirmReviewDto> GetFirmForReviewAsync(Guid firmId);
    Task ApproveFirmAsync(Guid firmId, Guid adminUserId);
    Task RejectFirmAsync(Guid firmId, Guid adminUserId, FirmRejectionReason reasonType, string reasonText);
}

public class AdminService(
    ApplicationDbContext context,
    IProfanityDetectionService profanityService,
    IRedisService redis,
    ILogger<AdminService> logger) : IAdminService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<List<FirmReviewListDto>> GetFirmsAwaitingReviewAsync(int page = 1, int pageSize = 20)
    {
        var skip = (page - 1) * pageSize;

        var firms = await context.Firms
            .Include(f => f.Media)
            .ThenInclude(fm => fm.Media)
            .Where(f => f.Status == FirmStatus.AwaitingReview)
            .OrderBy(f => f.SubmittedForReviewAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(f => new FirmReviewListDto
            {
                Id = f.Id,
                Name = f.Name,
                Cui = f.Cui,
                Status = f.Status,
                SubmittedForReviewAt = f.SubmittedForReviewAt,
                HasLogo = f.Media.Any(m => m.FirmMediaType == FirmMediaType.Logo && 
                                          m.Media.Status == MediaStatus.Available),
                HasCover = f.Media.Any(m => m.FirmMediaType == FirmMediaType.Cover && 
                                           m.Media.Status == MediaStatus.Available),
                ContainsPotentialProfanity = false // Will be checked on detail view
            })
            .ToListAsync();

        return firms;
    }

    public async Task<FirmReviewDto> GetFirmForReviewAsync(Guid firmId)
    {
        // Check cache first
        var cacheKey = $"admin:firm-review:{firmId}";
        var cached = await redis.GetAsync<FirmReviewDto>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var firm = await context.Firms
            .Include(f => f.Contact)
            .Include(f => f.Links)
            .Include(f => f.Location).ThenInclude(loc => loc!.Country)
            .Include(f => f.Location).ThenInclude(loc => loc!.County)
            .Include(f => f.Media).ThenInclude(fm => fm.Media)
            // include answers -> selected options
            .Include(f => f.Forms).ThenInclude(form => form.Answers).ThenInclude(answer => answer.SelectedOptions).ThenInclude( so => so.Option)
            // include answers -> question -> translations
            .Include(f => f.Forms).ThenInclude(form => form.Answers).ThenInclude(answer => answer.Question).ThenInclude(q => q.Translations)
            // include answers -> translations
            .Include(f => f.Forms).ThenInclude(form => form.Answers).ThenInclude(answer => answer.Translations) 
            .FirstOrDefaultAsync(f => f.Id == firmId);

        if (firm == null)
        {
            throw new NotFoundException("Firm", firmId.ToString());
        }

        // Check for profanity in all text fields
        var textsToCheck = new List<string>
        {
            firm.Name,
            firm.Description
        };

        textsToCheck.AddRange(
            firm.Forms
                .SelectMany(f => f.Answers)
                .Where(a => !string.IsNullOrWhiteSpace(a.Value))
                .Select(a => a.Value!));

        var profanityResult = await profanityService.CheckMultipleTextsAsync(textsToCheck);

        var dto = new FirmReviewDto
        {
            Id = firm.Id,
            Cui = firm.Cui,
            Name = firm.Name,
            Type = firm.Type.ToString(),
            Description = firm.Description,
            Status = firm.Status,
            SubmittedForReviewAt = firm.SubmittedForReviewAt,
            ContactEmail = firm.Contact?.Email,
            ContactPhone = firm.Contact?.Phone,
            Website = firm.Links?.Website,
            LinkedIn = firm.Links?.LinkedIn,
            Facebook = firm.Links?.Facebook,
            Twitter = firm.Links?.Twitter,
            Instagram = firm.Links?.Instagram,
            LocationAddress = firm.Location?.Address,
            LocationCity = firm.Location?.City ?? string.Empty,
            LocationPostalCode = firm.Location?.PostalCode,
            CountryName = firm.Location?.Country?.Name ?? string.Empty,
            CountyName = firm.Location?.County?.Name ?? string.Empty,
            ModerationResult = profanityResult,
            Media = firm.Media.Select(fm => new FirmMediaDto
            {
                Id = fm.Id,
                Type = fm.FirmMediaType,
                FileName = fm.Media.OriginalFileName,
                Url = fm.Media.S3KeyFinal ?? fm.Media.S3KeyTemp ?? string.Empty,
                SizeInBytes = fm.Media.SizeInBytes,
                Width = fm.Media.Width,
                Height = fm.Media.Height,
                Status = fm.Media.Status,
                CreatedAt = fm.Media.CreatedAt
            }).ToList(),
            Answers = firm.Forms
                .SelectMany(f => f.Answers)
                .Select(a => new FirmAnswerDto
                {
                    Id = a.Id,
                    Question = a.Question.Translations.First().Title, // Assuming at least one translation exists
                    Value = a.Value ?? a.Question.Type.ToString(),
                    Translations = a.Translations.Select(t => new FirmAnswerTranslationDto
                    {
                        LanguageCode = t.LanguageCode,
                        Value = t.Value
                    }).ToList(),
                    
                })
                .Where(a => !string.IsNullOrWhiteSpace(a.Value))
                .ToList()
        };

        // Cache the result
        await redis.SetAsync(cacheKey, dto, CacheDuration);

        return dto;
    }

    public async Task ApproveFirmAsync(Guid firmId, Guid adminUserId)
    {
        var firm = await context.Firms.FindAsync(firmId);
        
        if (firm == null)
        {
            throw new NotFoundException("Firm", firmId.ToString());
        }

        if (firm.Status != FirmStatus.AwaitingReview)
        {
            throw new InvalidOperationException($"Firm must be in AwaitingReview status to be approved. Current status: {firm.Status}");
        }

        firm.Status = FirmStatus.Approved;
        firm.ReviewedAt = DateTime.UtcNow;
        firm.ReviewedByUserId = adminUserId;
        firm.RejectionReason = null;
        firm.RejectionReasonType = null;

        await context.SaveChangesAsync();

        // Clear cache
        await redis.DeleteAsync($"admin:firm-review:{firmId}");
        await redis.DeleteAsync(CacheKeys.Firms.Info(firmId));

        logger.LogInformation("Firm {FirmId} approved by admin {AdminUserId}", firmId, adminUserId);
    }

    public async Task RejectFirmAsync(
        Guid firmId, 
        Guid adminUserId, 
        FirmRejectionReason reasonType, 
        string reasonText)
    {
        var firm = await context.Firms.FindAsync(firmId);
        
        if (firm == null)
        {
            throw new NotFoundException("Firm", firmId.ToString());
        }

        if (firm.Status != FirmStatus.AwaitingReview)
        {
            throw new InvalidOperationException($"Firm must be in AwaitingReview status to be rejected. Current status: {firm.Status}");
        }

        firm.Status = FirmStatus.Rejected;
        firm.ReviewedAt = DateTime.UtcNow;
        firm.ReviewedByUserId = adminUserId;
        firm.RejectionReasonType = reasonType;
        firm.RejectionReason = reasonText;

        await context.SaveChangesAsync();

        // Clear cache
        await redis.DeleteAsync($"admin:firm-review:{firmId}");
        await redis.DeleteAsync(CacheKeys.Firms.Info(firmId));

        logger.LogWarning("Firm {FirmId} rejected by admin {AdminUserId}. Reason: {Reason}", 
            firmId, adminUserId, reasonType);

        // TODO: Send notification email to firm about rejection
    }

   
}