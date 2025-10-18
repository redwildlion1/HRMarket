using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using HRMarket.Configuration.Types;
using HRMarket.Entities.Categories;

namespace HRMarket.Entities.Questions;

public class Question
{
    [Key] public required Guid Id { get; set; }

    public required string Title { get; set; }
    public QuestionType Type { get; init; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public bool IsFilter { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    [Column(TypeName = "jsonb")] public JsonDocument? ValidationJson { get; set; }

    public ICollection<Option> Options { get; set; } = [];

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    public ICollection<QuestionVariant> Variants { get; set; } = [];
}