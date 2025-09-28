using AutoMapper;
using HRMarket.Configuration.UniversalExtensions;
using HRMarket.Core.Questions.DTOs;
using Json.Schema;

namespace HRMarket.Core.Questions;

public interface IQuestionService
{
    public Task CreateForCategoryAsync(CreateQuestionsForCategoryDTO dto);
}

public class QuestionService(IQuestionRepository repository, IMapper mapper)
    : IQuestionService
{
    public Task CreateForCategoryAsync(CreateQuestionsForCategoryDTO dto)
    {
        // This will also check for duplicate orders or missing orders
        dto.Questions.CheckOrderedList();
        
        var choiceQuestions = dto.Questions
            .OfType<PostOptionQuestionDTO>();
        foreach (var question in choiceQuestions)
            question.Options.CheckOrderedList();
        
        var basicQuestions = dto.Questions
            .OfType<PostBasicQuestionDTO>();
        foreach (var question in basicQuestions)
            // If the JSON is invalid, an exception will be thrown
            JsonSchema.FromText(question.ValidationJson);
        
        var questions = mapper.Map<ICollection<Entities.Questions.Question>>(dto.Questions);
        
        // Set the CategoryId for each question
        foreach (var question in questions)
            question.CategoryId = dto.CategoryId;   
        
        return repository.AddQuestions(questions);
        
    }
}

