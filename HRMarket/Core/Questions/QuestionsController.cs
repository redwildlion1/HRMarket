using HRMarket.Core.Questions.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.Questions;

[ApiController]
[Route("api/questions")]
public class QuestionsController(IQuestionService service) : ControllerBase 
{
    [HttpPost("createForCategory")]
    public async Task<IActionResult> CreateForCategory([FromBody] CreateQuestionsForCategoryDto dto)
    {
        await service.CreateForCategoryAsync(dto);
        return Ok();
    }
}