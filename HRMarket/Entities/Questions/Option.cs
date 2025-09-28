using HRMarket.Entities.Answers;

namespace HRMarket.Entities.Questions;

public class Option
{
    public Guid Id { get; set; }
    
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    
    public string Label { get; set; } = null!;
    public string? Value { get; set; } 
    
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<Answer>? Answers { get; set; } = [];
}