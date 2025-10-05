namespace HRMarket.Core.Categories.DTOs;

public record PostClusterDTO(string Name, string Icon);
public record PostCategoryDTO(string Name, string Icon, int? OrderCluster = null, Guid? ClusterId = null);
public record PostServiceDTO(string Name, int OrderInCategory , Guid CategoryId);

public record AddCategoryToClusterDTO(Guid CategoryId, Guid ClusterId);