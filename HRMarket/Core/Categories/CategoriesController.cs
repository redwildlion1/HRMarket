using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Categories.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.Categories;

[ApiController]
[Route("api/categories")]
public class CategoriesController(
    ICategoryService service,
    ILanguageContext languageContext) : ControllerBase
{
    [HttpPost("clusters")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCluster([FromBody] PostClusterDto dto)
    {
        await service.CreateCluster(dto);
        return Ok(new { message = "Cluster created successfully" });
    }
    
    [HttpPost("categories")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] PostCategoryDto dto)
    {
        await service.CreateCategory(dto);
        return Ok(new { message = "Category created successfully" });
    }

    [HttpPost("services")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateService([FromBody] PostServiceDto dto)
    {
        await service.CreateService(dto);
        return Ok(new { message = "Service created successfully" });
    }

    [HttpPut("clusters/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCluster([FromRoute] Guid id, [FromBody] UpdateClusterDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "ID mismatch" });
        await service.UpdateCluster(dto);
        return Ok(new { message = "Cluster updated successfully" });
    }

    [HttpPut("categories/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory([FromRoute] Guid id, [FromBody] UpdateCategoryDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "ID mismatch" });

        await service.UpdateCategory(dto);
        return Ok(new { message = "Category updated successfully" });
    }

    [HttpPut("services/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateService([FromRoute] Guid id, [FromBody] UpdateServiceDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "ID mismatch" });

        await service.UpdateService(dto);
        return Ok(new { message = "Service updated successfully" });
    }
    
    [HttpPost("addCategoryToCluster")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddCategoryToCluster([FromBody] AddCategoryToClusterDto dto)
    {
        await service.AddCategoryToCluster(dto);
        return Ok(new { message = "Category added to cluster successfully" });
    }
    
    [HttpGet("clusters")]
    public async Task<IActionResult> GetFullClusters()
    {
        var clusters = await service.GetFullClusters(languageContext.Language);
        return Ok(clusters);
    }
    
    [HttpGet("categories/nocluster")]
    public async Task<IActionResult> GetCategoriesWithoutCluster()
    {
        var categories = await service.GetCategoriesWithoutCluster(languageContext.Language);
        return Ok(categories);
    }
}