using HRMarket.Entities;

namespace HRMarket.Validation.Extensions;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public class CheckConstraintsDb(ApplicationDbContext context)
{
    /// <summary>
    /// Validates EF Core model constraints (unique, required, max length).
    /// Uses Fluent API rules from DbContext and DisplayAttribute for messages.
    /// </summary>
    public async Task<List<string>> ValidateEntityAsync<TEntity>(TEntity? entity)
        where TEntity : class
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        var errors = new List<string>();

        if (entityType == null) return errors;
        
        if (entity == null)
        {
            errors.Add($"Câmpul „{entityType.Name}” este obligatoriu.");
            return errors;
        }

        // --- 1. REQUIRED + MAX LENGTH ---
        foreach (var property in entityType.GetProperties())
        {
            var propInfo = typeof(TEntity).GetProperty(property.Name);
            if (propInfo == null) continue;

            var value = propInfo.GetValue(entity);

            var displayName = propInfo.GetCustomAttribute<DisplayAttribute>()?.Name ?? property.Name;

            // Required
            if (!property.IsNullable && value == null)
            {
                errors.Add($"Câmpul „{displayName}” este obligatoriu.");
            }

            // MaxLength
            if (value is not string strVal || !property.GetMaxLength().HasValue) continue;
            var maxLength = property.GetMaxLength()!.Value;
            if (strVal.Length > maxLength)
            {
                errors.Add($"Câmpul „{displayName}” nu poate depăși {maxLength} caractere.");
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
                        dbSet = dbSet.Where(e => EF.Property<object>(e, pkProp.Name) != pkValue);
                    }
                }
            }

            if (!await dbSet.AnyAsync()) continue;
            // Use DisplayAttribute for user-friendly naming
            var propNames = properties
                .Select(p => typeof(TEntity).GetProperty(p.Name)?
                    .GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name);

            errors.Add($"Câmpul „{string.Join(", ", propNames)}” trebuie să fie unic. Valoarea introdusă există deja.");
        }

        return errors;
    }
}
