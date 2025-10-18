using FluentValidation;
using HRMarket.Core;

namespace HRMarket.Validation.Extensions;

public class EntityValidator(CheckConstraintsDb checker)
{
    /// <summary>
    /// Validate entity and throw FluentValidation exception if constraints fail.
    /// Uses language from DTO if it inherits from BaseDto.
    /// </summary>
    public async Task ValidateAndThrowAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        var language = GetLanguageFromDto(entity);
        var errors = await checker.ValidateEntityAsync(entity, language);

        if (errors.Count > 0)
        {
            ThrowValidationException(errors);
        }
    }

    /// <summary>
    /// Validate entity and nested entities, throwing FluentValidation exception if constraints fail.
    /// Uses language from the main DTO.
    /// </summary>
    public async Task ValidateAndThrowAsync<TEntity>(TEntity entity, params object?[] nestedEntities)
        where TEntity : class
    {
        var language = GetLanguageFromDto(entity);
        var allErrors = await checker.ValidateEntityAsync(entity, language);

        foreach (var nested in nestedEntities.Where(n => n != null))
        {
            var nestedErrors = await checker.ValidateEntityAsync(nested, language);
            foreach (var (key, value) in nestedErrors)
            {
                if (!allErrors.ContainsKey(key))
                {
                    allErrors[key] = [];
                }
                allErrors[key].AddRange(value);
            }
        }

        if (allErrors.Count > 0)
        {
            ThrowValidationException(allErrors);
        }
    }

    private static string GetLanguageFromDto(object? obj)
    {
        // Try to get language from BaseDto
        if (obj is BaseDto dto)
        {
            return string.IsNullOrWhiteSpace(dto.Language) ? "ro" : dto.Language.ToLower();
        }

        return "ro"; // Default to Romanian
    }

    private static void ThrowValidationException(Dictionary<string, List<string>> errors)
    {
        var validationFailures = errors.SelectMany(kvp =>
            kvp.Value.Select(errorMessage => new FluentValidation.Results.ValidationFailure(kvp.Key, errorMessage))
        ).ToList();

        throw new ValidationException(validationFailures);
    }
}