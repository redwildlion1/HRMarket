using HRMarket.Entities.Categories.Translations;

namespace HRMarket.Entities.Categories;

public class Service
{
    public Guid Id { get; set; }
    public int OrderInCategory { get; set; }
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    // Navigation properties
    public List<ServiceTranslation> Translations { get; set; } = [];
}