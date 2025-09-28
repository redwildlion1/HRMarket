using HRMarket.Core.Firms.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.Firms;

public class FirmController(IFirmService service) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateFirmDTO dto)
    {
        var result = await service.CreateAsync(dto);
        return Ok(result);
    }
}