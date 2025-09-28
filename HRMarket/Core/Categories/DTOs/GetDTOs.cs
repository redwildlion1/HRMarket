namespace HRMarket.Core.Categories.DTOs;

public record GetCategoryDTO(Guid Id, string Name, string Icon, int? OrderInCluster, Guid? ClusterId);
public record GetServiceDTO(Guid Id, string Name, int OrderInCategory, Guid CategoryId);
public record GetClusterDTO(Guid Id, string Name, int OrderInList, string Icon);
public record FullClusterDTO(Guid Id, string Name, int OrderInList, string Icon, 
    ICollection<GetCategoryWithServicesDTO> Categories);
    
public record GetCategoryWithServicesDTO(Guid Id, string Name, string Icon, int? OrderInCluster, Guid? ClusterId,
    ICollection<GetServiceDTO> Services);
    