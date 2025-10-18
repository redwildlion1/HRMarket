namespace HRMarket.Core.Categories.DTOs;

public record PostClusterDto(string Name, string Icon);
public record PostCategoryDto(string Name, string Icon, int? OrderCluster = null, Guid? ClusterId = null);
public record PostServiceDto(string Name, int OrderInCategory , Guid CategoryId);

public record AddCategoryToClusterDto(Guid CategoryId, Guid ClusterId);