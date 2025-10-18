using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HRMarket.Middleware;

/// <summary>
/// Action filter that validates request models using FluentValidation
/// before the action method is executed
/// </summary>
public class ValidateModelFilter(
    IServiceProvider serviceProvider,
    ILogger<ValidateModelFilter> logger)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        // Validate all action arguments
        foreach (var (parameterName, argumentValue) in context.ActionArguments)
        {
            if (argumentValue == null)
            {
                logger.LogDebug("Skipping validation for null parameter: {ParameterName}", parameterName);
                continue;
            }

            await ValidateArgumentAsync(argumentValue, parameterName, context);
        }

        // If validation passed, continue to the action method
        await next();
    }

    private async Task ValidateArgumentAsync(
        object argument, 
        string parameterName, 
        ActionExecutingContext context)
    {
        var argumentType = argument.GetType();

        // Skip validation for primitive types and strings
        if (argumentType.IsPrimitive || argumentType == typeof(string))
        {
            return;
        }

        // Try to get validator for this type
        var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

        if (serviceProvider.GetService(validatorType) is not IValidator validator)
        {
            logger.LogDebug(
                "No validator found for type {Type} on parameter {ParameterName}", 
                argumentType.Name, 
                parameterName);
            return;
        }

        // Perform validation
        var validationContext = new ValidationContext<object>(argument);
        var validationResult = await validator.ValidateAsync(validationContext);

        if (!validationResult.IsValid)
        {
            logger.LogWarning(
                "Validation failed for {Type} on action {Action}: {Errors}",
                argumentType.Name,
                context.ActionDescriptor.DisplayName,
                string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

            throw new ValidationException(validationResult.Errors);
        }

        logger.LogDebug(
            "Validation passed for {Type} on parameter {ParameterName}", 
            argumentType.Name, 
            parameterName);
    }
}