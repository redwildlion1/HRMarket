namespace HRMarket.Core.Auth;

public class RegisterDto(string email, string password, bool newsletter) : BaseDto
{
    public string Email { get; set; } = email;
    public string Password { get; set; } = password;
    public bool Newsletter { get; set; } = newsletter;
}

public class LoginResult(
    string token, 
    string refreshToken, 
    DateTime tokenExpires, 
    DateTime refreshTokenExpires,
    UserInfoDto userInfo) : BaseDto
{
    public string Token { get; set; } = token;
    public string RefreshToken { get; set; } = refreshToken;
    public DateTime TokenExpires { get; set; } = tokenExpires;
    public DateTime RefreshTokenExpires { get; set; } = refreshTokenExpires;
    public UserInfoDto UserInfo { get; set; } = userInfo;
}

public class UserInfoDto : BaseDto
{
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public bool IsAdmin { get; set; }
    public bool HasFirm { get; set; }
    public Guid? FirmId { get; set; }
    public string? FirmName { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class ContactPersonDto(string firstName, string lastName) : BaseDto
{
    public string FirstName { get; set; } = firstName;
    public string LastName { get; set; } = lastName;
}

public class RefreshTokenRequest(string token, string refreshToken) : BaseDto
{
    public string Token { get; set; } = token;
    public string RefreshToken { get; set; } = refreshToken;
}

public class ConfirmEmailRequest(Guid userId, string token) : BaseDto
{
    public Guid UserId { get; set; } = userId;
    public string Token { get; set; } = token;
}

public class CheckAdminResponse(bool isAdmin) : BaseDto
{
    public bool IsAdmin { get; set; } = isAdmin;
}

public class CheckFirmAccessResponse(bool canEdit, bool isFirmOwner) : BaseDto
{
    public bool CanEdit { get; set; } = canEdit;
    public bool IsFirmOwner { get; set; } = isFirmOwner;
}