namespace HRMarket.Entities.Questions;

public class OptionTranslation
{
    public Guid Id { get; set; }
    public Guid OptionId { get; set; }
    public QuestionOption Option { get; set; } = null!;
    
    public string LanguageCode { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string? Description { get; set; }
}