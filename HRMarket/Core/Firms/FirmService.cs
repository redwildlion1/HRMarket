using HRMarket.Configuration.Exceptions;
using HRMarket.Configuration.Status;
using HRMarket.Core.Auth;
using HRMarket.Core.Firms.DTOs;
using HRMarket.Core.Firms.Extensions;
using HRMarket.Entities.Firms;
using HRMarket.Validation.Extensions;
using Mapster;
using HRMarket.Configuration.Types;

namespace HRMarket.Core.Firms;

public interface IFirmService
{
    Task<Guid> CreateAsync(CreateFirmDto dto);
    Task SubmitFirmForReviewAsync(Guid firmId, Guid userId);
}

public class FirmService(
    IFirmRepository repository,
    IAuthService authService,
    EntityValidator validator,
    ILogger<FirmService> logger) : IFirmService
{
    public async Task<Guid> CreateAsync(CreateFirmDto dto)
    {
        var firm = dto.Adapt<Firm>();
        await validator.ValidateAndThrowAsync(firm, firm.Contact, firm.Links, firm.Location);
        return await repository.AddAsync(firm);
    }
    
    public async Task SubmitFirmForReviewAsync(Guid firmId, Guid userId)
    {
        var firm = repository.GetByIdAsync(firmId).WithMedia().FirstOrDefault();

        if (firm == null)
        {
            throw new NotFoundException("Firm", firmId.ToString());
        }
        

        var user = authService.GetUserById(userId);
        
        if (user == null)
        {
            throw new NotFoundException("User", userId.ToString());
        }
        

        // Verify both logo and cover are uploaded and available
        var hasLogo = firm.Media.Any(m => 
            m is { FirmMediaType: FirmMediaType.Logo, Media.Status: MediaStatus.Available });
            
        var hasCover = firm.Media.Any(m => 
            m is { FirmMediaType: FirmMediaType.Cover, Media.Status: MediaStatus.Available });

        if (!hasLogo || !hasCover)
        {
            throw new InvalidOperationException("Both logo and cover image must be uploaded before submitting for review");
        }

        if (firm.Status != FirmStatus.Draft)
        {
            throw new InvalidOperationException($"Firm cannot be submitted for review. Current status: {firm.Status}");
        }

        firm.Status = FirmStatus.AwaitingReview;
        firm.SubmittedForReviewAt = DateTime.UtcNow;

        await repository.UpdateAsync(firm);

        logger.LogInformation("Firm {FirmId} submitted for review by user {UserId}", firmId, userId);

        // TODO: Send notification to admins about new firm pending review
    }
}