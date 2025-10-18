// HRMarket/Validation/Extensions/CheckConstraintsDb.cs
using HRMarket.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using HRMarket.Configuration.Translation;

namespace HRMarket.Validation.Extensions;

public class CheckConstraintsDb(
    ApplicationDbContext context,
    ITranslationService translationService)
{
    /// <summary>
    /// Validates EF Core model constraints (unique, required, max length).
    /// Returns validation errors in the language specified.
    /// </summary>
    public async Task<Dictionary<string, List<string>>> ValidateEntityAsync<TEntity>(
        TEntity? entity, 
        string language = "ro")
        where TEntity : class
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        var errors = new Dictionary<string, List<string>>();

        if (entityType == null) return errors;
        
        if (entity == null)
        {
            AddError(errors, "Entity", 
                translationService.TranslateValidationError(ValidationErrorKeys.Required, language, entityType.Name));
            return errors;
        }

        // --- 1. REQUIRED + MAX LENGTH + MIN LENGTH ---
        foreach (var property in entityType.GetProperties())
        {
            var propInfo = typeof(TEntity).GetProperty(property.Name);
            if (propInfo == null) continue;

            var value = propInfo.GetValue(entity);
            var displayName = propInfo.GetCustomAttribute<DisplayAttribute>()?.Name ?? property.Name;
            var camelCasePropertyName = ToCamelCase(property.Name);

            // Required validation
            if (!property.IsNullable && value == null)
            {
                AddError(errors, camelCasePropertyName,
                    translationService.TranslateValidationError(ValidationErrorKeys.Required, language, displayName));
            }

            // String-specific validations
            if (value is not string strVal) continue;
            // MaxLength validation
            if (property.GetMaxLength().HasValue)
            {
                var maxLength = property.GetMaxLength()!.Value;
                if (strVal.Length > maxLength)
                {
                    AddError(errors, camelCasePropertyName,
                        translationService.TranslateValidationError(ValidationErrorKeys.MaxLength, language, displayName, maxLength));
                }
            }

            // MinLength validation
            var minLengthAttr = propInfo.GetCustomAttribute<MinLengthAttribute>();
            if (minLengthAttr != null && strVal.Length < minLengthAttr.Length)
            {
                AddError(errors, camelCasePropertyName,
                    translationService.TranslateValidationError(ValidationErrorKeys.MinLength, language, displayName, minLengthAttr.Length));
            }
        }

        // --- 2. UNIQUE CONSTRAINTS ---
        var uniqueIndexes = entityType.GetIndexes().Where(i => i.IsUnique);
        foreach (var index in uniqueIndexes)
        {
            var properties = index.Properties.ToList();
            var dbSet = context.Set<TEntity>().AsQueryable();

            foreach (var property in properties)
            {
                var propInfo = typeof(TEntity).GetProperty(property.Name);
                var value = propInfo?.GetValue(entity);
                // ReSharper disable once EntityFramework.ClientSideDbFunctionCall
                dbSet = dbSet.Where(e => EF.Property<object>(e, property.Name) == value);
            }

            // Exclude current entity (for updates)
            var pk = entityType.FindPrimaryKey();
            if (pk != null)
            {
                foreach (var pkProp in pk.Properties)
                {
                    var pkValue = typeof(TEntity).GetProperty(pkProp.Name)?.GetValue(entity);
                    if (pkValue != null)
                    {
                        // ReSharper disable once EntityFramework.ClientSideDbFunctionCall
                        dbSet = dbSet.Where(e => EF.Property<object>(e, pkProp.Name) != pkValue);
                    }
                }
            }

            if (!await dbSet.AnyAsync()) continue;

            // Build field names for error message
            var propNames = properties
                .Select(p => typeof(TEntity).GetProperty(p.Name)?
                    .GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name);

            var fieldName = string.Join(", ", propNames);
            var camelCasePropertyName = ToCamelCase(properties.First().Name);

            AddError(errors, camelCasePropertyName,
                translationService.TranslateValidationError(ValidationErrorKeys.Unique, language, fieldName));
        }

        return errors;
    }

    private static void AddError(Dictionary<string, List<string>> errors, string propertyName, string errorMessage)
    {
        if (!errors.TryGetValue(propertyName, out var value))
        {
            value = [];
            errors[propertyName] = value;
        }

        value.Add(errorMessage);
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}