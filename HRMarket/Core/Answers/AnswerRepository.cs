// HRMarket/Core/Answers/AnswerRepository.cs
using HRMarket.Entities;
using HRMarket.Entities.Answers;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Answers;

public interface IAnswerRepository
{
    Task<Answer> CreateAsync(Answer answer);
    Task<Answer?> GetByIdAsync(Guid answerId);
    Task<List<Answer>> GetByFormSubmissionIdAsync(Guid formSubmissionId);
    Task<Answer?> GetByQuestionAndFormSubmissionAsync(Guid questionId, Guid formSubmissionId);
    Task UpdateAsync(Answer answer);
    Task DeleteAsync(Guid answerId);
}

public class AnswerRepository(ApplicationDbContext context) : IAnswerRepository
{
    public async Task<Answer> CreateAsync(Answer answer)
    {
        context.Answers.Add(answer);
        await context.SaveChangesAsync();
        return answer;
    }
    
    public async Task<Answer?> GetByIdAsync(Guid answerId)
    {
        return await context.Answers
            .Include(a => a.Translations)
            .Include(a => a.SelectedOptions)
                .ThenInclude(ao => ao.Option)
                    .ThenInclude(o => o.Translations)
            .Include(a => a.Question)
                .ThenInclude(q => q.Translations)
            .FirstOrDefaultAsync(a => a.Id == answerId);
    }
    
    public async Task<List<Answer>> GetByFormSubmissionIdAsync(Guid formSubmissionId)
    {
        return await context.Answers
            .Include(a => a.Translations)
            .Include(a => a.SelectedOptions)
                .ThenInclude(ao => ao.Option)
                    .ThenInclude(o => o.Translations)
            .Include(a => a.Question)
                .ThenInclude(q => q.Translations)
            .Where(a => a.FormSubmissionId == formSubmissionId)
            .OrderBy(a => a.Question.Order)
            .ToListAsync();
    }
    
    public async Task<Answer?> GetByQuestionAndFormSubmissionAsync(Guid questionId, Guid formSubmissionId)
    {
        return await context.Answers
            .Include(a => a.Translations)
            .Include(a => a.SelectedOptions)
                .ThenInclude(ao => ao.Option)
                    .ThenInclude(o => o.Translations)
            .FirstOrDefaultAsync(a => a.QuestionId == questionId && a.FormSubmissionId == formSubmissionId);
    }
    
    public async Task UpdateAsync(Answer answer)
    {
        answer.UpdatedAt = DateTime.UtcNow;
        context.Answers.Update(answer);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(Guid answerId)
    {
        var answer = await context.Answers.FindAsync(answerId);
        if (answer != null)
        {
            context.Answers.Remove(answer);
            await context.SaveChangesAsync();
        }
    }
}