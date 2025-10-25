using System.Text.Json;
using HRMarket.Configuration.Exceptions;
using HRMarket.Configuration.Translation;
using HRMarket.Core.Questions.DTOs;
using HRMarket.Entities.Questions;

namespace HRMarket.Core.Questions;

public interface IQuestionService
{
    Task<QuestionDto> CreateQuestionAsync(Guid categoryId, CreateQuestionDto dto);
    Task<List<QuestionDto>> BulkCreateQuestionsAsync(BulkCreateQuestionsDto dto);
    Task<QuestionDto> UpdateQuestionAsync(UpdateQuestionDto dto);
    Task<QuestionDto?> GetQuestionAsync(Guid id, string languageCode);
    Task<QuestionListDto> GetQuestionsByCategoryAsync(Guid categoryId, string languageCode);
    Task DeleteQuestionAsync(Guid id);
}

public class QuestionService(
    IQuestionRepository questionRepository,
    ILanguageContext languageContext) : IQuestionService
{
    public async Task<QuestionDto> CreateQuestionAsync(Guid categoryId, CreateQuestionDto dto)
    {
        var question = new Question
        {
            CategoryId = categoryId,
            Type = dto.Type,
            Order = dto.Order,
            IsRequired = dto.IsRequired,
            IsFilter = dto.IsFilter,
            ValidationSchema = JsonDocument.Parse(dto.ValidationSchema)
        };
        
        // Add translations
        foreach (var translationDto in dto.Translations)
        {
            question.Translations.Add(new QuestionTranslation
            {
                Question = question,
                LanguageCode = translationDto.LanguageCode.ToLower(),
                Title = translationDto.Title,
                Description = translationDto.Description,
                Placeholder = translationDto.Placeholder
            });
        }
        
        // Add options for choice-based questions
        foreach (var optionDto in dto.Options)
        {
            var option = new QuestionOption
            {
                Question = question,
                Value = optionDto.Value,
                Order = optionDto.Order,
                Metadata = string.IsNullOrEmpty(optionDto.Metadata) 
                    ? null 
                    : JsonDocument.Parse(optionDto.Metadata)
            };
            
            foreach (var optionTranslationDto in optionDto.Translations)
            {
                option.Translations.Add(new OptionTranslation
                {
                    Option = option,
                    LanguageCode = optionTranslationDto.LanguageCode.ToLower(),
                    Label = optionTranslationDto.Label,
                    Description = optionTranslationDto.Description
                });
            }
            
            question.Options.Add(option);
        }
        
        var created = await questionRepository.CreateAsync(question);
        return MapToDto(created, languageContext.Language);
    }
    
    public async Task<List<QuestionDto>> BulkCreateQuestionsAsync(BulkCreateQuestionsDto dto)
    {
        var results = new List<QuestionDto>();
        
        foreach (var questionDto in dto.Questions)
        {
            var created = await CreateQuestionAsync(dto.CategoryId, questionDto);
            results.Add(created);
        }
        
        return results;
    }
    
    public async Task<QuestionDto> UpdateQuestionAsync(UpdateQuestionDto dto)
    {
        var question = await questionRepository.GetByIdAsync(dto.Id);
        if (question == null)
        {
            throw new NotFoundException($"Question with ID {dto.Id} not found");
        }
        
        question.Type = dto.Type;
        question.Order = dto.Order;
        question.IsRequired = dto.IsRequired;
        question.IsFilter = dto.IsFilter;
        question.ValidationSchema = JsonDocument.Parse(dto.ValidationSchema);
        
        // Update translations
        question.Translations.Clear();
        foreach (var translationDto in dto.Translations)
        {
            question.Translations.Add(new QuestionTranslation
            {
                Question = question,
                LanguageCode = translationDto.LanguageCode.ToLower(),
                Title = translationDto.Title,
                Description = translationDto.Description,
                Placeholder = translationDto.Placeholder
            });
        }
        
        // Update options
        question.Options.Clear();
        foreach (var optionDto in dto.Options)
        {
            var option = new QuestionOption
            {
                Question = question,
                Value = optionDto.Value,
                Order = optionDto.Order,
                Metadata = string.IsNullOrEmpty(optionDto.Metadata) 
                    ? null 
                    : JsonDocument.Parse(optionDto.Metadata)
            };
            
            foreach (var optionTranslationDto in optionDto.Translations)
            {
                option.Translations.Add(new OptionTranslation
                {
                    Option = option,
                    LanguageCode = optionTranslationDto.LanguageCode.ToLower(),
                    Label = optionTranslationDto.Label,
                    Description = optionTranslationDto.Description
                });
            }
            
            question.Options.Add(option);
        }
        
        var updated = await questionRepository.UpdateAsync(question);
        return MapToDto(updated, languageContext.Language);
    }
    
    public async Task<QuestionDto?> GetQuestionAsync(Guid id, string languageCode)
    {
        var question = await questionRepository.GetByIdAsync(id);
        return question == null ? null : MapToDto(question, languageCode);
    }
    
    public async Task<QuestionListDto> GetQuestionsByCategoryAsync(Guid categoryId, string languageCode)
    {
        var questions = await questionRepository.GetByCategoryIdAsync(categoryId);
        
        return new QuestionListDto
        {
            CategoryId = categoryId,
            Questions = questions.Select(q => MapToDto(q, languageCode)).ToList()
        };
    }
    
    public async Task DeleteQuestionAsync(Guid id)
    {
        await questionRepository.DeleteAsync(id);
    }
    
    private QuestionDto MapToDto(Question question, string languageCode)
    {
        var translation = question.Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode)
            ?? question.Translations.FirstOrDefault(t => t.LanguageCode == SupportedLanguages.English);
        
        if (translation == null)
        {
            throw new InvalidOperationException($"No translation found for question {question.Id}");
        }
        
        return new QuestionDto
        {
            Id = question.Id,
            Type = question.Type,
            Order = question.Order,
            IsRequired = question.IsRequired,
            Title = translation.Title,
            Description = translation.Description,
            Placeholder = translation.Placeholder,
            ValidationSchema = question.ValidationSchema,
            Options = question.Options
                .OrderBy(o => o.Order)
                .Select(o => MapOptionToDto(o, languageCode))
                .ToList()
        };
    }
    
    private static QuestionOptionDto MapOptionToDto(QuestionOption option, string languageCode)
    {
        var translation = option.Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode)
            ?? option.Translations.FirstOrDefault(t => t.LanguageCode == SupportedLanguages.English);
        
        if (translation == null)
        {
            throw new InvalidOperationException($"No translation found for option {option.Id}");
        }
        
        return new QuestionOptionDto
        {
            Id = option.Id,
            Value = option.Value,
            Order = option.Order,
            Label = translation.Label,
            Description = translation.Description,
            Metadata = option.Metadata
        };
    }
}
