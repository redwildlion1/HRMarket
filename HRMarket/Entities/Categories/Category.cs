using HRMarket.Entities.Firms;
using HRMarket.Entities.Questions;

namespace HRMarket.Entities.Categories;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Icon { get; set; } = null!;
    
    public int OrderInCluster { get; set; }
    
    public Guid? ClusterId { get; set; }
    public Cluster? Cluster { get; set; } 
    
    public List<Service> Services { get; set; } = [];
    public List<Question> Questions { get; set; } = [];
    public List<Firm>? Firms { get; set; } = [];
}