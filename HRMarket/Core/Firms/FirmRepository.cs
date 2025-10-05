using HRMarket.Entities;
using HRMarket.Entities.Firms;

namespace HRMarket.Core.Firms;

public interface IFirmRepository
{
    Task<Guid> AddAsync(Firm firm);
}

public class FirmRepository(ApplicationDbContext context) : IFirmRepository
{
    public async Task<Guid> AddAsync(Firm firm)
    {
        context.Add(firm);
        await context.SaveChangesAsync();
        return firm.Id;
    }
}