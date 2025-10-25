using HRMarket.Configuration.Exceptions;
using HRMarket.Entities;
using HRMarket.Entities.Categories;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Categories;

public interface ICategoriesRepository
{
    Task AddCluster(Cluster cluster);
    Task AddCategory(Category category);
    Task AddService(Service service);
    Task<ICollection<Cluster>> GetFullClusters(string languageCode);
    Task<ICollection<Category>> GetCategoriesWithoutCluster(string languageCode);
    Task AddCategoryToCluster(Guid categoryId, Guid clusterId);
    Task<Cluster?> GetClusterByIdAsync(Guid id);
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<Service?> GetServiceByIdAsync(Guid id);
    Task UpdateCluster(Cluster cluster);
    Task UpdateCategory(Category category);
    Task UpdateService(Service service);
}

public class CategoriesRepository(ApplicationDbContext context) : ICategoriesRepository
{
    public async Task AddCluster(Cluster cluster)
    {
        var maxOrder = await context.Set<Cluster>().MaxAsync(c => (int?)c.OrderInList) ?? 0;
        cluster.OrderInList = maxOrder + 1;
        
        await context.Set<Cluster>().AddAsync(cluster);
        await context.SaveChangesAsync();
    }

    public async Task AddCategory(Category category)
    {
        await context.Set<Category>().AddAsync(category);
        await context.SaveChangesAsync();
    }

    public async Task AddService(Service service)
    {
        await context.Set<Service>().AddAsync(service);
        await context.SaveChangesAsync();
    }

    public async Task<ICollection<Cluster>> GetFullClusters(string languageCode)
    {
        return await context.Set<Cluster>()
            .Include(c => c.Translations)
            .Include(c => c.Categories)
                .ThenInclude(cat => cat.Translations)
            .Include(c => c.Categories)
                .ThenInclude(cat => cat.Services)
                    .ThenInclude(s => s.Translations)
            .Where(c => c.IsActive)
            .OrderBy(c => c.OrderInList)
            .ToListAsync();
    }

    public async Task<ICollection<Category>> GetCategoriesWithoutCluster(string languageCode)
    {
        return await context.Set<Category>()
            .Include(c => c.Translations)
            .Include(c => c.Services)
                .ThenInclude(s => s.Translations)
            .Where(c => c.ClusterId == null)
            .ToListAsync();
    }

    public async Task AddCategoryToCluster(Guid categoryId, Guid clusterId)
    {
        var category = await context.Set<Category>().FirstOrDefaultAsync(c => c.Id == categoryId);
        if (category == null)
            throw new NotFoundException("Category", categoryId.ToString());
            
        category.ClusterId = clusterId;
        await context.SaveChangesAsync();
    }

    public async Task<Cluster?> GetClusterByIdAsync(Guid id)
    {
        return await context.Set<Cluster>()
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await context.Set<Category>()
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Service?> GetServiceByIdAsync(Guid id)
    {
        return await context.Set<Service>()
            .Include(s => s.Translations)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task UpdateCluster(Cluster cluster)
    {
        context.Set<Cluster>().Update(cluster);
        await context.SaveChangesAsync();
    }

    public async Task UpdateCategory(Category category)
    {
        context.Set<Category>().Update(category);
        await context.SaveChangesAsync();
    }

    public async Task UpdateService(Service service)
    {
        context.Set<Service>().Update(service);
        await context.SaveChangesAsync();
    }
}