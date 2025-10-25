using System.ComponentModel.DataAnnotations;
using HRMarket.Configuration;
using HRMarket.Configuration.Status;
using HRMarket.Configuration.Types;
using HRMarket.Entities.Categories;
using HRMarket.Entities.Medias;

namespace HRMarket.Entities.Firms;

public class Firm
{
    [Key]
    public Guid Id { get; init; }
    [StringLength(AppConstants.CuiLength)]
    public required string Cui { get; set; }
    [StringLength(AppConstants.MaxCompanyNameLength)]
    public required string Name { get; init; }
    public required FirmType Type { get; init; }
    public required string Description { get; set; }
    
    public FirmStatus Status { get; set; } = FirmStatus.Draft;
    public DateTime? SubmittedForReviewAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? RejectionReason { get; set; }
    public FirmRejectionReason? RejectionReasonType { get; set; }
    
    public List<Guid> CategoryIds { get; set; } = [];
    public List<Category>? Categories { get; set; }
    
    public List<FormForCategory> Forms { get; set; } = [];
    public FirmContact? Contact { get; set; }
    public FirmLinks? Links { get; set; }
    public FirmLocation? Location { get; set; }
    public List<FirmMedia> Media { get; set; } = [];
}