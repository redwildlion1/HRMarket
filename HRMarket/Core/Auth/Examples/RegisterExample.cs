using Swashbuckle.AspNetCore.Filters;

namespace HRMarket.Core.Auth.Examples;

public class RegisterExample : IExamplesProvider<RegisterDto>
{
    public RegisterDto GetExamples()
    {
        return new RegisterDto(
            email: "contact@thinkr.ro",
            password: "StrongPassword123!",
            newsletter: true
        );
    }
}