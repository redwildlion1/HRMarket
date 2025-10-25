using HRMarket.Entities.Questions;

namespace HRMarket.Entities.Answers;

public class AnswerOption
{
    public Guid AnswerId { get; set; }
    public Answer Answer { get; set; } = null!;
    
    public Guid OptionId { get; set; }
    public QuestionOption Option { get; set; } = null!;
    
    public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
}