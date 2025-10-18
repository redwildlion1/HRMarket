using HRMarket.Core.Categories.DTOs;
using Mapster;

namespace HRMarket.Core.Categories;

public interface ICategoryService
{
    Task CreateCluster(PostClusterDto dto);
    Task CreateCategory(PostCategoryDto dto);
    Task CreateService(PostServiceDto dto);
    Task<ICollection<FullClusterDto>> GetFullClusters();
    Task<ICollection<GetCategoryDto>> GetCategoriesWithoutCluster();
    Task AddCategoryToCluster(AddCategoryToClusterDto dto);
}

public class CategoryService(ICategoriesRepository repository) : ICategoryService
{
    public async Task CreateCluster(PostClusterDto dto)
    {
        var cluster = dto.Adapt<Entities.Categories.Cluster>();
        await repository.AddCluster(cluster);
    }

    public async Task CreateCategory(PostCategoryDto dto)
    {
        var category = dto.Adapt<Entities.Categories.Category>();
        await repository.AddCategory(category);
    }

    public async Task CreateService(PostServiceDto dto)
    {
        var service = dto.Adapt<Entities.Categories.Service>();
        await repository.AddService(service);
    }

    public async Task<ICollection<FullClusterDto>> GetFullClusters()
    {
        var clusters = await repository.GetFullClusters();
        return clusters.Adapt<ICollection<FullClusterDto>>();
    }

    public async Task<ICollection<GetCategoryDto>> GetCategoriesWithoutCluster()
    {
        var categories = await repository.GetCategoriesWithoutCluster();
        return categories.Adapt<ICollection<GetCategoryDto>>();
    }

    public Task AddCategoryToCluster(AddCategoryToClusterDto dto)
    {
        return repository.AddCategoryToCluster(dto.CategoryId, dto.ClusterId);
    }
}