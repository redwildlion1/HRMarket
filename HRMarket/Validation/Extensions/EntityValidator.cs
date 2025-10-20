using FluentValidation;
using HRMarket.Configuration.Translation;

namespace HRMarket.Validation.Extensions;

public class EntityValidator(
    CheckConstraintsDb checker,
    ILanguageContext languageContext)
{
    /// <summary>
    /// Validate entity and throw FluentValidation exception if constraints fail.
    /// </summary>
    public async Task ValidateAndThrowAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        var errors = await checker.ValidateEntityAsync(entity, languageContext.Language);

        if (errors.Count > 0)
        {
            ThrowValidationException(errors);
        }
    }

    /// <summary>
    /// Validate entity and nested entities, throwing FluentValidation exception if constraints fail.
    /// </summary>
    public async Task ValidateAndThrowAsync<TEntity>(TEntity entity, params object?[] nestedEntities)
        where TEntity : class
    {
        var allErrors = await checker.ValidateEntityAsync(entity, languageContext.Language);

        foreach (var nested in nestedEntities.Where(n => n != null))
        {
            var nestedErrors = await checker.ValidateEntityAsync(nested, languageContext.Language);
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

    private static void ThrowValidationException(Dictionary<string, List<string>> errors)
    {
        var validationFailures = errors.SelectMany(kvp =>
            kvp.Value.Select(errorMessage => new FluentValidation.Results.ValidationFailure(kvp.Key, errorMessage))
        ).ToList();

        throw new ValidationException(validationFailures);
    }
}