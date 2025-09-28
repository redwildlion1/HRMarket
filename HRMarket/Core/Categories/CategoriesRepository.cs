using HRMarket.Entities.Categories;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Categories;

public interface ICategoriesRepository
{
    Task AddCluster(Cluster cluster);
    Task AddCategory(Category category);
    Task AddService(Service service);
    Task<ICollection<Cluster>> GetFullClusters();
    Task<ICollection<Category>> GetCategoriesWithoutCluster();
}

public class CategoriesRepository(DbContext context) : ICategoriesRepository
{
    public async Task AddCluster(Cluster cluster)
    {
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

    public async Task<ICollection<Cluster>> GetFullClusters()
    {
        return await context.Set<Cluster>()
            .Include(c => c.Categories)
            .ThenInclude(cat => cat.Services)
            .ToListAsync();
    }

    public async Task<ICollection<Category>> GetCategoriesWithoutCluster()
    {
        return await context.Set<Category>()
            .Where(c => c.ClusterId == null)
            .Include(c => c.Services)
            .ToListAsync();
    }
}