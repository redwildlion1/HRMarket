using System.ComponentModel.DataAnnotations;
using HRMarket.Entities;

namespace HRMarket.Validation.Extensions;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class EntityValidator(ApplicationDbContext context)
{
    private readonly CheckConstraintsDb _checker = new(context);

    /// <summary>
    /// Validate entity and throw a user-friendly exception if constraints fail.
    /// </summary>
    public async Task ValidateAndThrowAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        var errors = await _checker.ValidateEntityAsync(entity);

        if (errors.Count > 0)
        {
            throw new ValidationException(string.Join("\n", errors));
        }
    }

    /// <summary>
    ///  Validate entity and nested entities, throwing a user-friendly exception if constraints fail.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="nestedEntities"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <exception cref="ValidationException"></exception>
    public async Task ValidateAndThrowAsync<TEntity>(TEntity entity, params object?[] nestedEntities)
        where TEntity : class
    {
        var errors = await _checker.ValidateEntityAsync(entity);

        foreach (var nested in nestedEntities)
        {
            var nestedErrors = await _checker.ValidateEntityAsync(nested);
            errors.AddRange(nestedErrors);
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(string.Join("\n", errors));
        }
    }
}