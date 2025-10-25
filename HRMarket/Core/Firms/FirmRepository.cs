using HRMarket.Entities;
using HRMarket.Entities.Firms;

namespace HRMarket.Core.Firms;

public interface IFirmRepository
{
    Task<Guid> AddAsync(Firm firm);
    IQueryable<Firm> GetByIdAsync(Guid firmId);
    Task UpdateAsync(Firm firm);
}

public class FirmRepository(ApplicationDbContext context) : IFirmRepository
{
    public async Task<Guid> AddAsync(Firm firm)
    {
        context.Add(firm);
        await context.SaveChangesAsync();
        return firm.Id;
    }

    public IQueryable<Firm> GetByIdAsync(Guid firmId)
    {
        return context.Firms.Where(f => f.Id == firmId);
    }

    public async Task UpdateAsync(Firm firm)
    {
        context.Firms.Update(firm);
        await context.SaveChangesAsync();
    }
}

