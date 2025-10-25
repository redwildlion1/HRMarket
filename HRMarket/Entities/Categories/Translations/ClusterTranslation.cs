namespace HRMarket.Entities.Categories.Translations;

public class ClusterTranslation
{
    public Guid Id { get; set; }
    public Guid ClusterId { get; set; }
    public Cluster Cluster { get; set; } = null!;
    
    public string LanguageCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}