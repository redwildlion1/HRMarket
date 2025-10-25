// HRMarket/Entities/Answers/Answer.cs
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using HRMarket.Entities.Firms;
using HRMarket.Entities.Questions;

namespace HRMarket.Entities.Answers;

public class Answer
{
    public Guid Id { get; set; }
    
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    
    // Composite FK to FormForCategory
    public Guid FirmId { get; set; }
    public Guid CategoryId { get; set; }
    public FormForCategory FormForCategory { get; set; } = null!;
    
    /// <summary>
    /// For Number, Date questions - store value here
    /// For String/Text questions - null (use Translations)
    /// For choice questions - null (use SelectedOptions)
    /// </summary>
    public string? Value { get; set; }
    
    public ICollection<AnswerOption> SelectedOptions { get; set; } = new List<AnswerOption>();
    
    /// <summary>
    /// ONLY for String/Text type questions
    /// </summary>
    public ICollection<AnswerTranslation> Translations { get; set; } = new List<AnswerTranslation>();
    
    [Column(TypeName = "jsonb")]
    public JsonDocument? StructuredData { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool NeedsUpdating { get; set; } = false;
}