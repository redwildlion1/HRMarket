using HRMarket.Core.Answers;
using HRMarket.Core.Firms.DTOs;
using HRMarket.Entities.Answers;
using HRMarket.Entities.Firms;
using HRMarket.Validation.Extensions;
using Mapster;

namespace HRMarket.Core.Firms;

public interface IFirmService
{
    Task<Guid> CreateAsync(CreateFirmDTO dto);
}

public class FirmService(
    IFirmRepository repository,
    EntityValidator validator) : IFirmService
{
    public async Task<Guid> CreateAsync(CreateFirmDTO dto)
    {
        var firm = dto.Adapt<Firm>();
        await validator.ValidateAndThrowAsync(firm, firm.Contact, firm.Links, firm.Location);
        return await repository.AddAsync(firm);
    }
}