using HRMarket.Core.Categories.DTOs;
using Mapster;

namespace HRMarket.Core.Categories;

public interface ICategoryService
{
    Task CreateCluster(PostClusterDTO dto);
    Task CreateCategory(PostCategoryDTO dto);
    Task CreateService(PostServiceDTO dto);
    Task<ICollection<FullClusterDTO>> GetFullClusters();
    Task<ICollection<GetCategoryDTO>> GetCategoriesWithoutCluster();
    Task AddCategoryToCluster(AddCategoryToClusterDTO dto);
}

public class CategoryService(ICategoriesRepository repository) : ICategoryService
{
    public async Task CreateCluster(PostClusterDTO dto)
    {
        var cluster = dto.Adapt<Entities.Categories.Cluster>();
        await repository.AddCluster(cluster);
    }

    public async Task CreateCategory(PostCategoryDTO dto)
    {
        var category = dto.Adapt<Entities.Categories.Category>();
        await repository.AddCategory(category);
    }

    public async Task CreateService(PostServiceDTO dto)
    {
        var service = dto.Adapt<Entities.Categories.Service>();
        await repository.AddService(service);
    }

    public async Task<ICollection<FullClusterDTO>> GetFullClusters()
    {
        var clusters = await repository.GetFullClusters();
        return clusters.Adapt<ICollection<FullClusterDTO>>();
    }

    public async Task<ICollection<GetCategoryDTO>> GetCategoriesWithoutCluster()
    {
        var categories = await repository.GetCategoriesWithoutCluster();
        return categories.Adapt<ICollection<GetCategoryDTO>>();
    }

    public Task AddCategoryToCluster(AddCategoryToClusterDTO dto)
    {
        return repository.AddCategoryToCluster(dto.CategoryId, dto.ClusterId);
    }
}