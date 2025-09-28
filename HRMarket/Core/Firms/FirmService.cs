using AutoMapper;
using HRMarket.Core.Answers;
using HRMarket.Core.Firms.DTOs;
using HRMarket.Entities.Answers;
using HRMarket.Entities.Firms;

namespace HRMarket.Core.Firms;

public interface IFirmService
{
    Task<Guid> CreateAsync(CreateFirmDTO dto);
}

public class FirmService(
    IMapper mapper,
    IFirmRepository repository,
    IAnswerService answerService) : IFirmService
{
    public async Task<Guid> CreateAsync(CreateFirmDTO dto)
    {
        var firm = await CreateFirmEntityAsync(dto);
        return await repository.AddAsync(firm);
    }

    private async Task<Firm> CreateFirmEntityAsync(CreateFirmDTO dto)
    {
        var firm = mapper.Map<Firm>(dto);

        var answers = dto.Form.Answers;
        var answersResult = await answerService.CheckAnswersAsync(answers);

        firm.FormSubmission = new FormSubmission()
        {
            IsCompleted = answersResult.IsComplete,
            Answers = mapper.Map<ICollection<Answer>>(answers)
        };
        
        return firm;
    }
}