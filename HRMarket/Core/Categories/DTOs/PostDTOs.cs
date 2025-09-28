namespace HRMarket.Core.Categories.DTOs;

public record PostClusterDTO(string Name, int OrderInList, string Icon);
public record PostCategoryDTO(string Name, string Icon, int? OrderCluster = null, Guid? ClusterId = null);
public record PostServiceDTO(string Name, int OrderInCategory , Guid CategoryId);