using HRMarket.Configuration.Moderation;
using HRMarket.Configuration.Status;
using HRMarket.Configuration.Types;

namespace HRMarket.Core.Admin;

public class FirmReviewDto : BaseDto
{
    public Guid Id { get; set; }
    public string Cui { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FirmStatus Status { get; set; }
    public DateTime? SubmittedForReviewAt { get; set; }
    
    // Contact
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    
    // Links
    public string? Website { get; set; }
    public string? LinkedIn { get; set; }
    public string? Facebook { get; set; }
    public string? Twitter { get; set; }
    public string? Instagram { get; set; }
    
    // Location
    public string? LocationAddress { get; set; }
    public string LocationCity { get; set; } = string.Empty;
    public string? LocationPostalCode { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string CountyName { get; set; } = string.Empty;
    
    // Media
    public List<FirmMediaDto> Media { get; set; } = [];
    
    // Form Submission
    public bool IsFormComplete { get; set; }
    public List<FirmAnswerDto> Answers { get; set; } = [];
    
    // Moderation flags
    public ProfanityCheckResult? ModerationResult { get; set; }
}

public class FirmMediaDto
{
    public Guid Id { get; set; }
    public FirmMediaType Type { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public MediaStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FirmAnswerDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public List<FirmAnswerTranslationDto> Translations { get; set; } = [];
    
}

public class FirmAnswerTranslationDto
{
    public string LanguageCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class FirmReviewListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Cui { get; set; } = string.Empty;
    public FirmStatus Status { get; set; }
    public DateTime? SubmittedForReviewAt { get; set; }
    public bool HasLogo { get; set; }
    public bool HasCover { get; set; }
    public bool ContainsPotentialProfanity { get; set; }
}

public class ApproveFirmDto : BaseDto
{
    public Guid FirmId { get; set; }
}

public class RejectFirmDto : BaseDto
{
    public Guid FirmId { get; set; }
    public FirmRejectionReason ReasonType { get; set; }
    public string ReasonText { get; set; } = string.Empty;
}

public class SubmitFirmForReviewDto : BaseDto
{
    public Guid FirmId { get; set; }
}