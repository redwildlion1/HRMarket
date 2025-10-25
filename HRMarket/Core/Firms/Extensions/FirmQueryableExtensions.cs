using HRMarket.Entities.Firms;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Firms.Extensions;

public static class FirmQueryableExtensions
{
    public static IQueryable<Firm> WithMedia(this IQueryable<Firm> query)
    {
        return query.Include(f => f.Media);
    }
}