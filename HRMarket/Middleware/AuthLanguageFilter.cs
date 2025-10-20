using HRMarket.Core;
using HRMarket.Core.Auth.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HRMarket.Middleware;

/// <summary>
/// Extracts language from BaseDto and sets it in AuthLanguageContext
/// Runs before model validation and action execution
/// </summary>
public class AuthLanguageFilter(IAuthLanguageContext languageContext) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        // Extract language from any BaseDto in action arguments
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is not BaseDto dto || string.IsNullOrWhiteSpace(dto.Language)) continue;
            languageContext.Language = dto.Language.ToLower();
            break; // Use first BaseDto found
        }

        await next();
    }
}