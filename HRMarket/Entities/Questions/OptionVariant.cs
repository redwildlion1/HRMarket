using System.ComponentModel.DataAnnotations;
using HRMarket.Entities.Languages;

namespace HRMarket.Entities.Questions;

public class OptionVariant
{
    [Key] public required Guid Id { get; set; }
    public Guid OptionId { get; set; }
    public string LanguageId { get; set; } = null!;
    public required string Value { get; set; }

    public Option Option { get; set; } = null!;
    public Language Language { get; set; } = null!;
}