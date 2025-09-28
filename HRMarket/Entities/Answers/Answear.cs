using HRMarket.Entities.Firms;
using HRMarket.Entities.Questions;

namespace HRMarket.Entities.Answers;

public class Answer
{
    public Guid Id { get; set; }
    public required string Question { get; set; }
    public required Guid QuestionId { get; set; }
    
    public bool NeedsUpdating { get; set; } = false;
    public string? Value { get; set; } = null;
    
    public Guid FormSubmissionId { get; set; }
    public FormSubmission? FormSubmission { get; set; }
    
    public ICollection<Option>? SelectedOptions { get; set; } = [];
}