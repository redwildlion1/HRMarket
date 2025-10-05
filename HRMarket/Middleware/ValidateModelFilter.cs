namespace HRMarket.Middleware;

using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

public class ValidateModelFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg == null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());

            if (serviceProvider.GetService(validatorType) is not IValidator validator) continue;
            var validationResult = await validator.ValidateAsync(new ValidationContext<object>(arg));
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        await next();
    }

}
