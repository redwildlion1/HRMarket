using System.ComponentModel.DataAnnotations;
using HRMarket.Configuration;

namespace HRMarket.Entities.Auth;

public class RefreshToken
{
    [Key]
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    [StringLength(AppConstants.MaxTokenLength)]
    public required string Token { get; init; }
    public DateTime Expires { get; init; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public DateTime Created { get; init; } = DateTime.UtcNow;
    [StringLength(AppConstants.MaxIpLength)]
    public string? CreatedByIp { get; init; }
    public DateTime? Revoked { get; set; }
    [StringLength(AppConstants.MaxIpLength)]
    public string? RevokedByIp { get; set; }
    public bool IsActive => Revoked == null && !IsExpired;
    
    public User User { get; init; } = null!;
}