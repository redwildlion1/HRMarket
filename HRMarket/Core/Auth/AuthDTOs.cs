namespace HRMarket.Core.Auth;

public record RegisterDTO(string Email, string Password, bool Newsletter);
public record LoginResult(string Token, string RefreshToken, DateTime TokenExpires, DateTime RefreshTokenExpires);
public record ContactPersonDTO(string FirstName, string LastName);
public record RefreshTokenRequest(string Token, string RefreshToken);