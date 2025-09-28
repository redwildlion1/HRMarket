using HRMarket.Entities.Questions;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Questions;

public interface IQuestionRepository
{
    public Task<ICollection<Question>> AddQuestions(ICollection<Question> questions);
    public Task<ICollection<Question>> GetByIdsAsync(IEnumerable<Guid> questionsIds);
}

public class QuestionRepository(DbContext context) : IQuestionRepository
{
    public async Task<ICollection<Question>> AddQuestions(ICollection<Question> questions)
    {
        context.AddRange(questions);
        await context.SaveChangesAsync();
        return questions;
    }

    public async Task<ICollection<Question>> GetByIdsAsync(IEnumerable<Guid> questionsIds)
    {
        return await context.Set<Question>()
            .Where(q => questionsIds.Contains(q.Id))
            .ToListAsync();
    }
}