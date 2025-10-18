namespace HRMarket.Core.Categories.DTOs;

public class PostClusterDto(string name, string icon) : BaseDto
{
    public string Name { get; } = name;
    public string Icon { get; } = icon;
}
public class PostCategoryDto(string name, string icon, int? orderCluster = null, Guid? clusterId = null) : BaseDto
{
    public string Name { get; } = name;
    public string Icon { get; } = icon;
    public int? OrderInCluster { get; } = orderCluster;
    public Guid? ClusterId { get; } = clusterId;
}
public class PostServiceDto(string name, int orderInCategory , Guid categoryId) : BaseDto
{
    public string Name { get; } = name;
    public int OrderInCategory { get; } = orderInCategory;
    public Guid CategoryId { get; } = categoryId;
}

public class AddCategoryToClusterDto(Guid categoryId, Guid clusterId) : BaseDto
{
    public Guid CategoryId { get; } = categoryId;
    public Guid ClusterId { get; } = clusterId;
}