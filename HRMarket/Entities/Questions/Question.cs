// HRMarket/Entities/Questions/Question.cs
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using HRMarket.Configuration.Types;
using HRMarket.Entities.Categories;

namespace HRMarket.Entities.Questions;

public class Question
{
    public Guid Id { get; set; }
    public QuestionType Type { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public bool IsFilter { get; set; }
    
    [Column(TypeName = "jsonb")]
    public JsonDocument ValidationSchema { get; set; } = null!;
    
    public ICollection<QuestionTranslation> Translations { get; set; } = new List<QuestionTranslation>();
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}