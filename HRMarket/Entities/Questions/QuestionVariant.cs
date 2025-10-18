using System.ComponentModel.DataAnnotations;
using HRMarket.Entities.Languages;

namespace HRMarket.Entities.Questions;

public class QuestionVariant
{
    [Key] public required Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string LanguageId { get; set; } = null!;
    public required string Value { get; set; }
    
    public Question Question { get; set; } = null!;
    public Language Language { get; set; } = null!;
}