namespace HRMarket.Entities.Categories;

public class Service
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    
    public int OrderInCategory { get; set; }
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}