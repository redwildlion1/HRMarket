namespace HRMarket.Core.Categories.DTOs;

public record GetCategoryDto(Guid Id, string Name, string Icon, int? OrderInCluster, Guid? ClusterId);
public record GetServiceDto(Guid Id, string Name, int OrderInCategory, Guid CategoryId);
public record GetClusterDto(Guid Id, string Name, int OrderInList, string Icon);
public record FullClusterDto(Guid Id, string Name, int OrderInList, string Icon, 
    ICollection<GetCategoryWithServicesDto> Categories);
    
public record GetCategoryWithServicesDto(Guid Id, string Name, string Icon, int? OrderInCluster, Guid? ClusterId,
    ICollection<GetServiceDto> Services);
    