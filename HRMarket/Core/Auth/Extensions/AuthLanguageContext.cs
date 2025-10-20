namespace HRMarket.Core.Auth.Extensions;
/// <summary>
/// Scoped service that holds the current request's language
/// Automatically populated from BaseDto by middleware
/// </summary>
public interface IAuthLanguageContext
{
    string Language { get; set; }
}

public class AuthLanguageContext : IAuthLanguageContext
{
    public string Language { get; set; } = "ro";
}