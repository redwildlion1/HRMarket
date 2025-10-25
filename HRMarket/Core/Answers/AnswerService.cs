// HRMarket/Core/Answers/AnswerService.cs
using System.Text.Json;
using HRMarket.Configuration.Translation;
using HRMarket.Configuration.Types;
using HRMarket.Core.Questions;
using HRMarket.Entities.Answers;
using HRMarket.Entities.Questions;
using Json.Schema;

namespace HRMarket.Core.Answers;

public interface IAnswerService
{
    Task<SubmitAnswersResultDto> SubmitAnswersAsync(SubmitAnswersDto dto);
    Task<List<AnswerDto>> GetAnswersAsync(Guid formSubmissionId, string languageCode);
    Task<AnswerDto?> GetAnswerAsync(Guid answerId, string languageCode);
}

public class AnswerService(
    IAnswerRepository answerRepository,
    IQuestionRepository questionRepository,
    ITranslationService translationService,
    ILanguageContext languageContext,
    ILogger<AnswerService> logger) : IAnswerService
{
    public async Task<SubmitAnswersResultDto> SubmitAnswersAsync(SubmitAnswersDto dto)
    {
        var result = new SubmitAnswersResultDto { IsValid = true };
        
        var questionIds = dto.Answers.Select(a => a.QuestionId).Distinct().ToList();
        var questions = await questionRepository.GetByIdsAsync(questionIds);
        var questionDict = questions.ToDictionary(q => q.Id);
        
        // Validate
        foreach (var answerDto in dto.Answers)
        {
            if (!questionDict.TryGetValue(answerDto.QuestionId, out var question))
            {
                result.IsValid = false;
                result.Errors.Add(new AnswerValidationError
                {
                    QuestionId = answerDto.QuestionId,
                    QuestionTitle = "Unknown Question",
                    ErrorMessages = ["Question not found"]
                });
                continue;
            }
            
            var validationErrors = await ValidateAnswerAsync(answerDto, question);
            if (validationErrors.Count <= 0) continue;
            result.IsValid = false;
            result.Errors.Add(new AnswerValidationError
            {
                QuestionId = question.Id,
                QuestionTitle = GetQuestionTitle(question, languageContext.Language),
                ErrorMessages = validationErrors
            });
        }
        
        if (!result.IsValid)
        {
            return result;
        }
        
        // Create answers
        foreach (var answerDto in dto.Answers)
        {
            var question = questionDict[answerDto.QuestionId];
            
            // Check if answer already exists (update scenario)
            var existingAnswer = await answerRepository.GetByQuestionAndFormSubmissionAsync(
                answerDto.QuestionId, 
                dto.CategoryId);
            
            if (existingAnswer != null)
            {
                await UpdateAnswerAsync(existingAnswer, answerDto, question);
                result.CreatedAnswerIds.Add(existingAnswer.Id);
            }
            else
            {
                var answer = await CreateAnswerAsync(answerDto, question, dto.CategoryId);
                result.CreatedAnswerIds.Add(answer.Id);
            }
        }
        
        return result;
    }
    
    private async Task<Answer> CreateAnswerAsync(
        SubmitAnswerDto dto, 
        Question question, 
        Guid formSubmissionId)
    {
        var answer = new Answer
        {
            QuestionId = dto.QuestionId,
            FormSubmissionId = formSubmissionId,
            StructuredData = string.IsNullOrEmpty(dto.StructuredData) 
                ? null 
                : JsonDocument.Parse(dto.StructuredData)
        };
        
        switch (question.Type)
        {
            case QuestionType.String:
            case QuestionType.Text:
                await AddStringAnswerTranslationsAsync(answer, dto.Value!, languageContext.Language);
                break;
                
            case QuestionType.Number:
            case QuestionType.Date:
                answer.Value = dto.Value;
                break;
                
            case QuestionType.SingleSelect:
            case QuestionType.MultiSelect:
                foreach (var optionId in dto.SelectedOptionIds)
                {
                    answer.SelectedOptions.Add(new AnswerOption
                    {
                        Answer = answer,
                        OptionId = optionId
                    });
                }
                break;
        }
        
        return await answerRepository.CreateAsync(answer);
    }
    
    private async Task UpdateAnswerAsync(Answer answer, SubmitAnswerDto dto, Question question)
    {
        answer.StructuredData = string.IsNullOrEmpty(dto.StructuredData) 
            ? null 
            : JsonDocument.Parse(dto.StructuredData);
        
        switch (question.Type)
        {
            case QuestionType.String:
            case QuestionType.Text:
                answer.Translations.Clear();
                await AddStringAnswerTranslationsAsync(answer, dto.Value!, languageContext.Language);
                break;
                
            case QuestionType.Number:
            case QuestionType.Date:
                answer.Value = dto.Value;
                break;
                
            case QuestionType.SingleSelect:
            case QuestionType.MultiSelect:
                answer.SelectedOptions.Clear();
                foreach (var optionId in dto.SelectedOptionIds)
                {
                    answer.SelectedOptions.Add(new AnswerOption
                    {
                        Answer = answer,
                        OptionId = optionId
                    });
                }
                break;
        }
        
        await answerRepository.UpdateAsync(answer);
    }
    
    private async Task AddStringAnswerTranslationsAsync(
        Answer answer, 
        string value, 
        string requestLanguage)
    {
        var languagesToStore = SupportedLanguages.GetStorageLanguages(requestLanguage);
        
        foreach (var languageCode in languagesToStore)
        {
            string translatedValue;
            
            if (languageCode == SupportedLanguages.English)
            {
                translatedValue = requestLanguage == SupportedLanguages.English
                    ? value
                    : await translationService.TranslateTextAsync(
                        value, 
                        requestLanguage, 
                        SupportedLanguages.English);
            }
            else
            {
                translatedValue = value;
            }
            
            answer.Translations.Add(new AnswerTranslation
            {
                Answer = answer,
                LanguageCode = languageCode,
                Value = translatedValue
            });
        }
    }
    
    public async Task<List<AnswerDto>> GetAnswersAsync(Guid formSubmissionId, string languageCode)
    {
        var answers = await answerRepository.GetByFormSubmissionIdAsync(formSubmissionId);
        var questions = await questionRepository.GetByIdsAsync(answers.Select(a => a.QuestionId).ToList());
        var questionDict = questions.ToDictionary(q => q.Id);
        
        return answers.Select(a => MapToDto(a, questionDict[a.QuestionId], languageCode)).ToList();
    }
    
    public async Task<AnswerDto?> GetAnswerAsync(Guid answerId, string languageCode)
    {
        var answer = await answerRepository.GetByIdAsync(answerId);
        if (answer == null) return null;
        
        var question = await questionRepository.GetByIdAsync(answer.QuestionId);
        if (question == null) return null;
        
        return MapToDto(answer, question, languageCode);
    }
    
    private AnswerDto MapToDto(Answer answer, Question question, string languageCode)
    {
        var dto = new AnswerDto
        {
            Id = answer.Id,
            QuestionId = answer.QuestionId,
            QuestionType = question.Type,
            CreatedAt = answer.CreatedAt,
            UpdatedAt = answer.UpdatedAt,
            NeedsUpdating = answer.NeedsUpdating
        };
        
        switch (question.Type)
        {
            case QuestionType.String:
            case QuestionType.Text:
                var translation = answer.Translations
                    .FirstOrDefault(t => t.LanguageCode == languageCode)
                    ?? answer.Translations.FirstOrDefault(t => t.LanguageCode == SupportedLanguages.English);
                dto.Value = translation?.Value;
                break;
                
            case QuestionType.Number:
            case QuestionType.Date:
                dto.Value = answer.Value;
                break;
                
            case QuestionType.SingleSelect:
            case QuestionType.MultiSelect:
                dto.SelectedOptions = answer.SelectedOptions.Select(ao => new SelectedOptionDto
                {
                    OptionId = ao.OptionId,
                    Value = ao.Option.Value,
                    Label = GetOptionLabel(ao.Option, languageCode),
                    SelectedAt = ao.SelectedAt
                }).ToList();
                break;
        }
        
        return dto;
    }
    
    private Task<List<string>> ValidateAnswerAsync(SubmitAnswerDto dto, Question question)
    {
        var errors = new List<string>();
        
        if (question.IsRequired)
        {
            var hasValue = question.Type switch
            {
                QuestionType.SingleSelect or QuestionType.MultiSelect => dto.SelectedOptionIds.Count > 0,
                _ => !string.IsNullOrWhiteSpace(dto.Value)
            };
            
            if (!hasValue)
            {
                errors.Add(translationService.Translate(
                    ValidationErrorKeys.Required, 
                    languageContext.Language, 
                    GetQuestionTitle(question, languageContext.Language)));
                return Task.FromResult(errors);
            }
        }
        
        try
        {
            var schema = JsonSchema.FromText(question.ValidationSchema.RootElement.GetRawText());
            
            switch (question.Type)
            {
                case QuestionType.String:
                case QuestionType.Text:
                case QuestionType.Number:
                case QuestionType.Date:
                {
                    var jsonValue = JsonSerializer.Serialize(dto.Value);
                    var evaluation = schema.Evaluate(jsonValue);
                    if (!evaluation.IsValid)
                    {
                        errors.Add(translationService.Translate(
                            ValidationErrorKeys.InvalidFormat,
                            languageContext.Language,
                            GetQuestionTitle(question, languageContext.Language)));
                    }
                    break;
                }
                
                case QuestionType.SingleSelect:
                case QuestionType.MultiSelect:
                {
                    var optionIdsJson = JsonSerializer.Serialize(dto.SelectedOptionIds);
                    var evaluation = schema.Evaluate(optionIdsJson);
                    if (!evaluation.IsValid)
                    {
                        errors.Add(translationService.Translate(
                            ValidationErrorKeys.InvalidOption,
                            languageContext.Language));
                    }
                    
                    var validOptionIds = question.Options.Select(o => o.Id).ToHashSet();
                    var invalidOptions = dto.SelectedOptionIds.Where(id => !validOptionIds.Contains(id)).ToList();
                    if (invalidOptions.Count > 0)
                    {
                        errors.Add(translationService.Translate(
                            ValidationErrorKeys.InvalidOption,
                            languageContext.Language));
                    }
                    break;
                }
                default:
                    throw new NotSupportedException($"Unsupported question type: {question.Type}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating answer for question {QuestionId}", question.Id);
            errors.Add("Validation error occurred");
        }
        
        return Task.FromResult(errors);
    }
    
    private static string GetQuestionTitle(Question question, string languageCode)
    {
        var translation = question.Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode)
            ?? question.Translations.FirstOrDefault(t => t.LanguageCode == SupportedLanguages.English);
        
        return translation?.Title ?? "Unknown Question";
    }
    
    private static string GetOptionLabel(QuestionOption option, string languageCode)
    {
        var translation = option.Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode)
            ?? option.Translations.FirstOrDefault(t => t.LanguageCode == SupportedLanguages.English);
        
        return translation?.Label ?? option.Value;
    }
}