// HRMarket/Core/Answers/AnswerRepository.cs
using HRMarket.Entities;
using HRMarket.Entities.Answers;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Answers;

public interface IAnswerRepository
{
    Task<Answer> CreateAsync(Answer answer);
    Task<Answer?> GetByIdAsync(Guid answerId);
    Task<List<Answer>> GetByFormSubmissionAsync(Guid firmId, Guid categoryId);
    Task<Answer?> GetByQuestionAndFormSubmissionAsync(Guid questionId, Guid firmId, Guid categoryId);
    Task<bool> FormSubmissionExistsAsync(Guid firmId, Guid categoryId);
    Task UpdateAsync(Answer answer);
    Task UpdateRangeAsync(IEnumerable<Answer> answers);
    Task DeleteAsync(Guid answerId);
    Task DeleteRangeAsync(IEnumerable<Guid> answerIds);
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
            .Include(a => a.Question)
                .ThenInclude(q => q.Options)
                    .ThenInclude(o => o.Translations)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == answerId);
    }
    
    public async Task<List<Answer>> GetByFormSubmissionAsync(Guid firmId, Guid categoryId)
    {
        return await context.Answers
            .Include(a => a.Translations)
            .Include(a => a.SelectedOptions)
                .ThenInclude(ao => ao.Option)
                    .ThenInclude(o => o.Translations)
            .Include(a => a.Question)
                .ThenInclude(q => q.Translations)
            .Include(a => a.Question)
                .ThenInclude(q => q.Options)
                    .ThenInclude(o => o.Translations)
            .Where(a => a.FirmId == firmId && a.CategoryId == categoryId)
            .OrderBy(a => a.Question.Order)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<Answer?> GetByQuestionAndFormSubmissionAsync(
        Guid questionId, 
        Guid firmId, 
        Guid categoryId)
    {
        return await context.Answers
            .Include(a => a.Translations)
            .Include(a => a.SelectedOptions)
                .ThenInclude(ao => ao.Option)
                    .ThenInclude(o => o.Translations)
            .Include(a => a.Question)
                .ThenInclude(q => q.Translations)
            .Include(a => a.Question)
                .ThenInclude(q => q.Options)
                    .ThenInclude(o => o.Translations)
            .FirstOrDefaultAsync(a => 
                a.QuestionId == questionId && 
                a.FirmId == firmId && 
                a.CategoryId == categoryId);
    }
    
    public async Task<bool> FormSubmissionExistsAsync(Guid firmId, Guid categoryId)
    {
        return await context.FormSubmissions
            .AnyAsync(f => f.FirmId == firmId && f.CategoryId == categoryId);
    }
    
    public async Task UpdateAsync(Answer answer)
    {
        answer.UpdatedAt = DateTime.UtcNow;
        context.Answers.Update(answer);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateRangeAsync(IEnumerable<Answer> answers)
    {
        var now = DateTime.UtcNow;
        foreach (var answer in answers)
        {
            answer.UpdatedAt = now;
            context.Answers.Update(answer);
        }
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
    
    public async Task DeleteRangeAsync(IEnumerable<Guid> answerIds)
    {
        var answers = await context.Answers
            .Where(a => answerIds.Contains(a.Id))
            .ToListAsync();
            
        if (answers.Any())
        {
            context.Answers.RemoveRange(answers);
            await context.SaveChangesAsync();
        }
    }
}