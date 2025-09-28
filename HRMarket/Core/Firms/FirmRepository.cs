using HRMarket.Entities.Firms;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Firms;

public interface IFirmRepository
{
    Task<Guid> AddAsync(Firm firm);
}

public class FirmRepository(DbContext context) : IFirmRepository
{
    public async Task<Guid> AddAsync(Firm firm)
    {
        context.Add(firm);
        await context.SaveChangesAsync();
        return firm.Id;
    }
}