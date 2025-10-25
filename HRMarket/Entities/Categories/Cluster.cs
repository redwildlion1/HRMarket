using HRMarket.Entities.Categories.Translations;

namespace HRMarket.Entities.Categories;

public class Cluster
{
    public Guid Id { get; set; }
    public string Icon { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public int OrderInList { get; set; }
    
    // Navigation properties
    public List<ClusterTranslation> Translations { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
}