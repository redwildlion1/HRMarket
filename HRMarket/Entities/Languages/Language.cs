using System.ComponentModel.DataAnnotations;

namespace HRMarket.Entities.Languages;

public class Language
{
    [Key]
    public required string Id { get; set; }
    public required string Name { get; set; }
}