using HRMarket.Entities.Languages;

namespace HRMarket.Entities.Answers;

public class AnswerVariant
{
    public Guid Id { get; set; }
    public Guid AnswerId { get; set; }
    public string LanguageId { get; set; } = null!;
    public required string Value { get; set; }

    public Answer Answer { get; set; } = null!;
    public Language Language { get; set; } = null!;
}