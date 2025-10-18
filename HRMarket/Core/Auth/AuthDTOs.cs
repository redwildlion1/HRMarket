namespace HRMarket.Core.Auth;

public class RegisterDto(string email, string password, bool newsletter) : BaseDto
{
    public string Email { get; set; } = email;
    public string Password { get; set; } = password;
    public bool Newsletter { get; set; } = newsletter;
};
public class LoginResult(string token, string refreshToken, DateTime tokenExpires, DateTime refreshTokenExpires) : BaseDto
{
    public string Token { get; set; } = token;
    public string RefreshToken { get; set; } = refreshToken;
    public DateTime TokenExpires { get; set; } = tokenExpires;
    public DateTime RefreshTokenExpires { get; set; } = refreshTokenExpires;
};
public class ContactPersonDto(string firstName, string lastName) : BaseDto
{
    public string FirstName { get; set; } = firstName;
    public string LastName { get; set; } = lastName;
};
public class RefreshTokenRequest(string token, string refreshToken) : BaseDto
{
    public string Token { get; set; } = token;
    public string RefreshToken { get; set; } = refreshToken;
};