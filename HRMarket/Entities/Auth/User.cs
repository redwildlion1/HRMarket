using Microsoft.AspNetCore.Identity;

namespace HRMarket.Entities.Auth;

public class User : IdentityUser<Guid>
{
    public bool Newsletter { get; set; }
    public bool IsFirm { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}

