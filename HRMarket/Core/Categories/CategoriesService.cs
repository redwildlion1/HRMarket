using AutoMapper;
using HRMarket.Core.Categories.DTOs;

namespace HRMarket.Core.Categories;

public interface ICategoryService
{
    Task CreateCluster(PostClusterDTO dto);
    Task CreateCategory(PostCategoryDTO dto);
    Task CreateService(PostServiceDTO dto);
    Task<ICollection<FullClusterDTO>> GetFullClusters();
    Task<ICollection<GetCategoryDTO>> GetCategoriesWithoutCluster();
}

public class CategoryService(ICategoriesRepository repository, IMapper mapper) : ICategoryService
{
    public async Task CreateCluster(PostClusterDTO dto)
    {
        var cluster = mapper.Map<Entities.Categories.Cluster>(dto);
        await repository.AddCluster(cluster);
    }

    public async Task CreateCategory(PostCategoryDTO dto)
    {
        var category = mapper.Map<Entities.Categories.Category>(dto);
        await repository.AddCategory(category);
    }

    public async Task CreateService(PostServiceDTO dto)
    {
        var service = mapper.Map<Entities.Categories.Service>(dto);
        await repository.AddService(service);
    }

    public async Task<ICollection<FullClusterDTO>> GetFullClusters()
    {
        var clusters = await repository.GetFullClusters();
        return mapper.Map<ICollection<FullClusterDTO>>(clusters);
    }

    public async Task<ICollection<GetCategoryDTO>> GetCategoriesWithoutCluster()
    {
        var categories = await repository.GetCategoriesWithoutCluster();
        return mapper.Map<ICollection<GetCategoryDTO>>(categories);
    }
}