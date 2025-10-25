using FluentValidation;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Questions;
using HRMarket.Core.Questions.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMarket.Controllers.Admin;

[ApiController]
[Route("api/admin/questions")]
[Authorize(Roles = "Admin")]
public class AdminQuestionsController(
    IQuestionService questionService,
    IValidator<CreateQuestionDto> createValidator,
    IValidator<BulkCreateQuestionsDto> bulkValidator,
    ILanguageContext languageContext) : ControllerBase
{
    [HttpPost("categories/{categoryId}")]
    public async Task<ActionResult<QuestionDto>> CreateQuestion(
        Guid categoryId,
        [FromBody] CreateQuestionDto dto)
    {
        var validationResult = await createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        
        var result = await questionService.CreateQuestionAsync(categoryId, dto);
        return CreatedAtAction(nameof(GetQuestion), new { id = result.Id }, result);
    }
    
    [HttpPost("bulk")]
    public async Task<ActionResult<List<QuestionDto>>> BulkCreateQuestions(
        [FromBody] BulkCreateQuestionsDto dto)
    {
        var validationResult = await bulkValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        
        var results = await questionService.BulkCreateQuestionsAsync(dto);
        return Ok(results);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<QuestionDto>> UpdateQuestion(
        Guid id,
        [FromBody] UpdateQuestionDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest("ID mismatch");
        }
        
        var result = await questionService.UpdateQuestionAsync(dto);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionDto>> GetQuestion(Guid id)
    {
        var result = await questionService.GetQuestionAsync(id, languageContext.Language);
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
    [HttpGet("categories/{categoryId}")]
    public async Task<ActionResult<QuestionListDto>> GetQuestionsByCategory(Guid categoryId)
    {
        var result = await questionService.GetQuestionsByCategoryAsync(categoryId, languageContext.Language);
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        await questionService.DeleteQuestionAsync(id);
        return NoContent();
    }
}