using Swashbuckle.AspNetCore.Filters;

namespace HRMarket.Core.Auth.Examples;

public class RegisterExample : IExamplesProvider<RegisterDto>
{
    public RegisterDto GetExamples()
    {
        return new RegisterDto(
            Email: "contact@thinkr.ro",
            Password: "StrongPassword123!",
            Newsletter: true
        );
    }
}