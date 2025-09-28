using Swashbuckle.AspNetCore.Filters;

namespace HRMarket.Core.Auth.Examples;

public class RegisterExample : IExamplesProvider<RegisterDTO>
{
    public RegisterDTO GetExamples()
    {
        return new RegisterDTO(
            Email: "contact@thinkr.ro",
            Password: "StrongPassword123!",
            Newsletter: true
        );
    }
}