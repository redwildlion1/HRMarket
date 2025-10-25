// HRMarket/Core/Questions/QuestionRepository.cs
using HRMarket.Entities;
using HRMarket.Entities.Questions;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Questions;

public interface IQuestionRepository
{
    Task<Question> CreateAsync(Question question);
    Task<Question?> GetByIdAsync(Guid id);
    Task<List<Question>> GetByIdsAsync(List<Guid> ids);
    Task<List<Question>> GetByCategoryIdAsync(Guid categoryId);
    Task<Question> UpdateAsync(Question question);
    Task DeleteAsync(Guid id);
}

public class QuestionRepository(ApplicationDbContext context) : IQuestionRepository
{
    public async Task<Question> CreateAsync(Question question)
    {
        context.Questions.Add(question);
        await context.SaveChangesAsync();
        return question;
    }
    
    public async Task<Question?> GetByIdAsync(Guid id)
    {
        return await context.Questions
            .Include(q => q.Translations)
            .Include(q => q.Options)
                .ThenInclude(o => o.Translations)
            .FirstOrDefaultAsync(q => q.Id == id && q.IsActive);
    }
    
    public async Task<List<Question>> GetByIdsAsync(List<Guid> ids)
    {
        return await context.Questions
            .Include(q => q.Translations)
            .Include(q => q.Options)
                .ThenInclude(o => o.Translations)
            .Where(q => ids.Contains(q.Id) && q.IsActive)
            .ToListAsync();
    }
    
    public async Task<List<Question>> GetByCategoryIdAsync(Guid categoryId)
    {
        return await context.Questions
            .Include(q => q.Translations)
            .Include(q => q.Options)
                .ThenInclude(o => o.Translations)
            .Where(q => q.CategoryId == categoryId && q.IsActive)
            .OrderBy(q => q.Order)
            .ToListAsync();
    }
    
    public async Task<Question> UpdateAsync(Question question)
    {
        question.UpdatedAt = DateTime.UtcNow;
        context.Questions.Update(question);
        await context.SaveChangesAsync();
        return question;
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var question = await context.Questions.FindAsync(id);
        if (question != null)
        {
            question.IsActive = false;
            question.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}