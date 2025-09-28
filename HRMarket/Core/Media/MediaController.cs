using HRMarket.Configuration.Types;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.Media;

[ApiController]
[Route("api/media/firms/{firmId:guid}")]
public class MediaController(IFileUploadBuilderFactory fileUploadBuilderFactory)
    : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(15_000_000)]
    public async Task<IActionResult> Upload([FromRoute] Guid firmId, IFormFile file, string type)
    {
        var userId = Guid.Parse(User.Identity!.Name!);
        var firmIdGuid = Guid.Parse(User.Claims.First(c => c.Type == "FirmId").Value);
        if (firmId != firmIdGuid)
        {
            return Forbid();
        }

        var builder = fileUploadBuilderFactory.Create(file, userId)
            .ForFirm(firmId, Enum.Parse<FirmMediaType>(type, true))
            .WithAllowedTypes([
                FileType.Png,
                FileType.Jpeg
            ]);
        
        return Ok(new
        {
            Status = await builder.SaveAsync()
        });
    }
}