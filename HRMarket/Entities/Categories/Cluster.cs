namespace HRMarket.Entities.Categories;

public class Cluster
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public bool IsActive { get; set; }
    
    public int OrderInList { get; set; }
    
    public List<Category> Categories { get; set; } = [];
}