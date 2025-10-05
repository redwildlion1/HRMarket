using HRMarket.Core.Categories.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.Categories;

public class CategoriesController(ICategoryService service) : ControllerBase
{
    [HttpPost("clusters")]
    public async Task<IActionResult> CreateCluster(PostClusterDTO dto)
    {
        await service.CreateCluster(dto);
        return Ok();
    }
    
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(PostCategoryDTO inClusterDTO)
    {
        await service.CreateCategory(inClusterDTO);
        return Ok();
    }

    [HttpPost("services")]
    public async Task<IActionResult> CreateService(PostServiceDTO dto)
    {
        await service.CreateService(dto);
        return Ok();
    }
    
    [HttpPost("categories/nocluster")]
    public async Task<IActionResult> CreateCategoryWithoutCluster(PostCategoryDTO dto)
    {
        await service.CreateCategory(dto);
        return Ok();
    }
    
    [HttpGet("categories/nocluster")]
    public async Task<IActionResult> GetCategoriesWithoutCluster()
    {
        await service.GetCategoriesWithoutCluster();
        return Ok();
    }
    
    [HttpPost("addCategoryToCluster")]
    public async Task<IActionResult> AddCategoryToCluster(AddCategoryToClusterDTO dto)
    {
        await service.AddCategoryToCluster(dto);
        return Ok();
    }
    
    [HttpGet("clusters")]
    public async Task<IActionResult> GetFullClusters()
    {
        await service.GetFullClusters();
        return Ok();
    }
    
}