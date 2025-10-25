using HRMarket.Configuration.Exceptions;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Categories.DTOs;
using HRMarket.Entities.Categories;
using HRMarket.Entities.Categories.Translations;

namespace HRMarket.Core.Categories;

public interface ICategoryService
{
    Task CreateCluster(PostClusterDto dto);
    Task CreateCategory(PostCategoryDto dto);
    Task CreateService(PostServiceDto dto);
    Task<ICollection<FullClusterDto>> GetFullClusters(string languageCode);
    Task<ICollection<GetCategoryDto>> GetCategoriesWithoutCluster(string languageCode);
    Task AddCategoryToCluster(AddCategoryToClusterDto dto);
    Task UpdateCluster(UpdateClusterDto dto);
    Task UpdateCategory(UpdateCategoryDto dto);
    Task UpdateService(UpdateServiceDto dto);
}

public class CategoryService(
    ICategoriesRepository repository,
    ILanguageContext languageContext) : ICategoryService
{
    public async Task CreateCluster(PostClusterDto dto)
    {
        var cluster = new Cluster
        {
            Icon = dto.Icon,
            IsActive = true
        };

        foreach (var translation in dto.Translations)
        {
            cluster.Translations.Add(new ClusterTranslation
            {
                Cluster = cluster,
                LanguageCode = translation.LanguageCode.ToLower(),
                Name = translation.Name,
                Description = translation.Description
            });
        }

        await repository.AddCluster(cluster);
    }

    public async Task CreateCategory(PostCategoryDto dto)
    {
        var category = new Category
        {
            Icon = dto.Icon,
            OrderInCluster = dto.OrderInCluster ?? 0,
            ClusterId = dto.ClusterId
        };

        foreach (var translation in dto.Translations)
        {
            category.Translations.Add(new CategoryTranslation
            {
                Category = category,
                LanguageCode = translation.LanguageCode.ToLower(),
                Name = translation.Name,
                Description = translation.Description
            });
        }

        await repository.AddCategory(category);
    }

    public async Task CreateService(PostServiceDto dto)
    {
        var service = new Service
        {
            OrderInCategory = dto.OrderInCategory,
            CategoryId = dto.CategoryId
        };

        foreach (var translation in dto.Translations)
        {
            service.Translations.Add(new ServiceTranslation
            {
                Service = service,
                LanguageCode = translation.LanguageCode.ToLower(),
                Name = translation.Name,
                Description = translation.Description
            });
        }

        await repository.AddService(service);
    }

    public async Task<ICollection<FullClusterDto>> GetFullClusters(string languageCode)
    {
        var clusters = await repository.GetFullClusters(languageCode);
        
        return clusters.Select(c => new FullClusterDto(
            c.Id,
            GetTranslatedName(c.Translations, languageCode),
            GetTranslatedDescription(c.Translations, languageCode),
            c.OrderInList,
            c.Icon,
            c.IsActive,
            c.Categories
                .OrderBy(cat => cat.OrderInCluster)
                .Select(cat => new GetCategoryWithServicesDto(
                    cat.Id,
                    GetTranslatedName(cat.Translations, languageCode),
                    GetTranslatedDescription(cat.Translations, languageCode),
                    cat.Icon,
                    cat.OrderInCluster,
                    cat.ClusterId,
                    cat.Services
                        .OrderBy(s => s.OrderInCategory)
                        .Select(s => new GetServiceDto(
                            s.Id,
                            GetTranslatedName(s.Translations, languageCode),
                            GetTranslatedDescription(s.Translations, languageCode),
                            s.OrderInCategory,
                            s.CategoryId
                        ))
                        .ToList()
                ))
                .ToList()
        )).ToList();
    }

    public async Task<ICollection<GetCategoryDto>> GetCategoriesWithoutCluster(string languageCode)
    {
        var categories = await repository.GetCategoriesWithoutCluster(languageCode);
        
        return categories.Select(c => new GetCategoryDto(
            c.Id,
            GetTranslatedName(c.Translations, languageCode),
            GetTranslatedDescription(c.Translations, languageCode),
            c.Icon,
            c.OrderInCluster,
            c.ClusterId
        )).ToList();
    }

    public Task AddCategoryToCluster(AddCategoryToClusterDto dto)
    {
        return repository.AddCategoryToCluster(dto.CategoryId, dto.ClusterId);
    }

    public async Task UpdateCluster(UpdateClusterDto dto)
    {
        var cluster = await repository.GetClusterByIdAsync(dto.Id);
        if (cluster == null)
            throw new NotFoundException("Cluster", dto.Id.ToString());

        cluster.Icon = dto.Icon;
        cluster.IsActive = dto.IsActive;
        
        // Clear existing translations and add new ones
        cluster.Translations.Clear();
        foreach (var translation in dto.Translations)
        {
            cluster.Translations.Add(new ClusterTranslation
            {
                Cluster = cluster,
                LanguageCode = translation.LanguageCode.ToLower(),
                Name = translation.Name,
                Description = translation.Description
            });
        }

        await repository.UpdateCluster(cluster);
    }

    public async Task UpdateCategory(UpdateCategoryDto dto)
    {
        var category = await repository.GetCategoryByIdAsync(dto.Id);
        if (category == null)
            throw new NotFoundException("Category", dto.Id.ToString());

        category.Icon = dto.Icon;
        category.OrderInCluster = dto.OrderInCluster ?? 0;
        category.ClusterId = dto.ClusterId;
        
        // Clear existing translations and add new ones
        category.Translations.Clear();
        foreach (var translation in dto.Translations)
        {
            category.Translations.Add(new CategoryTranslation
            {
                Category = category,
                LanguageCode = translation.LanguageCode.ToLower(),
                Name = translation.Name,
                Description = translation.Description
            });
        }

        await repository.UpdateCategory(category);
    }

    public async Task UpdateService(UpdateServiceDto dto)
    {
        var service = await repository.GetServiceByIdAsync(dto.Id);
        if (service == null)
            throw new NotFoundException("Service", dto.Id.ToString());

        service.OrderInCategory = dto.OrderInCategory;
        service.CategoryId = dto.CategoryId;
        
        // Clear existing translations and add new ones
        service.Translations.Clear();
        foreach (var translation in dto.Translations)
        {
            service.Translations.Add(new ServiceTranslation
            {
                Service = service,
                LanguageCode = translation.LanguageCode.ToLower(),
                Name = translation.Name,
                Description = translation.Description
            });
        }

        await repository.UpdateService(service);
    }

    private static string GetTranslatedName<T>(ICollection<T> translations, string languageCode) where T : class
    {
        var translation = translations.FirstOrDefault(t => 
            GetLanguageCode(t) == languageCode.ToLower()) 
            ?? translations.FirstOrDefault(t => 
                GetLanguageCode(t) == SupportedLanguages.English);
        
        return translation != null ? GetName(translation) : string.Empty;
    }

    private static string? GetTranslatedDescription<T>(ICollection<T> translations, string languageCode) where T : class
    {
        var translation = translations.FirstOrDefault(t => 
            GetLanguageCode(t) == languageCode.ToLower()) 
            ?? translations.FirstOrDefault(t => 
                GetLanguageCode(t) == SupportedLanguages.English);
        
        return translation != null ? GetDescription(translation) : null;
    }

    private static string GetLanguageCode(object translation)
    {
        var property = translation.GetType().GetProperty("LanguageCode");
        return property?.GetValue(translation)?.ToString()?.ToLower() ?? string.Empty;
    }

    private static string GetName(object translation)
    {
        var property = translation.GetType().GetProperty("Name");
        return property?.GetValue(translation)?.ToString() ?? string.Empty;
    }

    private static string? GetDescription(object translation)
    {
        var property = translation.GetType().GetProperty("Description");
        return property?.GetValue(translation)?.ToString();
    }
}