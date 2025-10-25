namespace HRMarket.Entities.Answers;

public class AnswerTranslation
{
    public Guid Id { get; set; }
    public Guid AnswerId { get; set; }
    public Answer Answer { get; set; } = null!;
    
    public string LanguageCode { get; set; } = null!;
    public string Value { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}