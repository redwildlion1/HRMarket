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
    ILanguageContext languageContext,
    ILogger<AnswersController> logger) : ControllerBase
{
    /// <summary>
    /// Submit or update answers for a form submission
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubmitAnswersResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    
    /// <summary>
    /// Get all answers for a form submission
    /// </summary>
    [HttpGet("firms/{firmId:guid}/categories/{categoryId:guid}")]
    [ProducesResponseType(typeof(List<AnswerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<AnswerDto>>> GetAnswers(
        Guid firmId, 
        Guid categoryId)
    {
            var results = await answerService.GetAnswersAsync(
                firmId, 
                categoryId, 
                languageContext.Language);
            return Ok(results);
    }
    
    /// <summary>
    /// Get a specific answer by ID
    /// </summary>
    [HttpGet("{answerId:guid}")]
    [ProducesResponseType(typeof(AnswerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AnswerDto>> GetAnswer(Guid answerId)
    {
            var result = await answerService.GetAnswerAsync(answerId, languageContext.Language);
            if (result == null)
            {
                return NotFound();
            }
            
            return Ok(result);
    }
    
    /// <summary>
    /// Delete a specific answer
    /// </summary>
    [HttpDelete("{answerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAnswer(Guid answerId)
    {
            var deleted = await answerService.DeleteAnswerAsync(answerId);
            if (!deleted)
            {
                return NotFound();
            }
            
            return NoContent();
    }
    
    /// <summary>
    /// Delete all answers for a form submission
    /// </summary>
    [HttpDelete("firms/{firmId:guid}/categories/{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAnswersForFormSubmission(
        Guid firmId, 
        Guid categoryId)
    {
            var deleted = await answerService.DeleteAnswersForFormSubmissionAsync(firmId, categoryId);
            if (!deleted)
            {
                return NotFound();
            }
            
            return NoContent();
    }
}