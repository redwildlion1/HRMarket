using HRMarket.Configuration.Translation;
using HRMarket.Core.Questions.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.Questions;

[ApiController]
[Route("api/questions")]
public class QuestionsController(
    IQuestionService questionService,
    ILanguageContext languageContext) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestionDto>> GetQuestion(Guid id)
    {
        var result = await questionService.GetQuestionAsync(id, languageContext.Language);
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
    [HttpGet("categories/{categoryId:guid}")]
    public async Task<ActionResult<QuestionListDto>> GetQuestionsByCategory(Guid categoryId)
    {
        var result = await questionService.GetQuestionsByCategoryAsync(categoryId, languageContext.Language);
        return Ok(result);
    }
}