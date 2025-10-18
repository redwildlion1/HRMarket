using HRMarket.Core.Firms.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.Firms;

[ApiController]
[Route("api/firms")]
public class FirmController(IFirmService service) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateFirmDto dto)
    {
        var result = await service.CreateAsync(dto);
        return Ok(result);
    }
}