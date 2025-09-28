using HRMarket.Core.Answers;

namespace HRMarket.Core.Firms.DTOs;

public class FormDTO(List<AnswerDTO> answers)
{
    public List<AnswerDTO> Answers { get; set; } = answers;
}