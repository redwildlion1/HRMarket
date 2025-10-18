using HRMarket.Core.Categories.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.Categories;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService service) : ControllerBase
{
    [HttpPost("clusters")]
    public async Task<IActionResult> CreateCluster(PostClusterDto dto)
    {
        await service.CreateCluster(dto);
        return Ok();
    }
    
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(PostCategoryDto inClusterDto)
    {
        await service.CreateCategory(inClusterDto);
        return Ok();
    }

    [HttpPost("services")]
    public async Task<IActionResult> CreateService(PostServiceDto dto)
    {
        await service.CreateService(dto);
        return Ok();
    }
    
    [HttpPost("categories/nocluster")]
    public async Task<IActionResult> CreateCategoryWithoutCluster(PostCategoryDto dto)
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
    public async Task<IActionResult> AddCategoryToCluster(AddCategoryToClusterDto dto)
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