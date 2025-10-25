// HRMarket/Controllers/AnswersController.cs

using FluentValidation;
using HRMarket.Configuration.Translation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Core.Answers;

[ApiController]
[Route("api/answers")]
[Authorize]
public class AnswersController(
    IAnswerService answerService,
    IValidator<SubmitAnswersDto> validator,
    ILanguageContext languageContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SubmitAnswersResultDto>> SubmitAnswers(
        [FromBody] SubmitAnswersDto dto)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        
        var result = await answerService.SubmitAnswersAsync(dto);
        
        if (!result.IsValid)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
    
    [HttpGet("submissions/{formSubmissionId:guid}")]
    public async Task<ActionResult<List<AnswerDto>>> GetAnswers(Guid formSubmissionId)
    {
        var results = await answerService.GetAnswersAsync(formSubmissionId, languageContext.Language);
        return Ok(results);
    }
    
    [HttpGet("{answerId:guid}")]
    public async Task<ActionResult<AnswerDto>> GetAnswer(Guid answerId)
    {
        var result = await answerService.GetAnswerAsync(answerId, languageContext.Language);
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
}