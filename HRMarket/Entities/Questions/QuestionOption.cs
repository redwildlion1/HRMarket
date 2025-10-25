using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace HRMarket.Entities.Questions;

public class QuestionOption
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    
    public string Value { get; set; } = null!;
    public int Order { get; set; }
    
    public ICollection<OptionTranslation> Translations { get; set; } = new List<OptionTranslation>();
    
    [Column(TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }
    
    public bool IsActive { get; set; } = true;
}