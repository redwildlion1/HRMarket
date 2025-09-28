using HRMarket.Core.Questions.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.Questions;

public class QuestionsController(IQuestionService service) : ControllerBase 
{
    [HttpPost("createForCategory")]
    public async Task<IActionResult> CreateForCategory([FromBody] CreateQuestionsForCategoryDTO dto)
    {
        await service.CreateForCategoryAsync(dto);
        return Ok();
    }
}