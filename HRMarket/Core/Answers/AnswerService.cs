// HRMarket/Core/Answers/AnswerService.cs
using System.Text.Json;
using HRMarket.Configuration.Translation;
using HRMarket.Configuration.Types;
using HRMarket.Core.Questions;
using HRMarket.Entities.Answers;
using HRMarket.Entities.Questions;
using Json.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.Answers;

public interface IAnswerService
{
    Task<SubmitAnswersResultDto> SubmitAnswersAsync(SubmitAnswersDto dto);
    Task<List<AnswerDto>> GetAnswersAsync(Guid firmId, Guid categoryId, string languageCode);
    Task<AnswerDto?> GetAnswerAsync(Guid answerId, string languageCode);
    Task<bool> DeleteAnswerAsync(Guid answerId);
    Task<bool> DeleteAnswersForFormSubmissionAsync(Guid firmId, Guid categoryId);
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
        
        try
        {
            // Validate form submission exists
            var formExists = await answerRepository.FormSubmissionExistsAsync(dto.FirmId, dto.CategoryId);
            if (!formExists)
            {
                logger.LogWarning(
                    "Form submission not found for FirmId: {FirmId}, CategoryId: {CategoryId}", 
                    dto.FirmId, 
                    dto.CategoryId);
                    
                result.IsValid = false;
                result.Errors.Add(new AnswerValidationError
                {
                    QuestionId = Guid.Empty,
                    QuestionTitle = "Form Submission",
                    ErrorMessages = new List<string> { "Form submission not found" }
                });
                return result;
            }
            
            // Get all questions
            var questionIds = dto.Answers.Select(a => a.QuestionId).Distinct().ToList();
            if (!questionIds.Any())
            {
                result.IsValid = false;
                result.Errors.Add(new AnswerValidationError
                {
                    QuestionId = Guid.Empty,
                    QuestionTitle = "Answers",
                    ErrorMessages = new List<string> { "No answers provided" }
                });
                return result;
            }
            
            var questions = await questionRepository.GetByIdsAsync(questionIds);
            var questionDict = questions.ToDictionary(q => q.Id);
            
            // Validate all answers first
            foreach (var answerDto in dto.Answers)
            {
                if (!questionDict.TryGetValue(answerDto.QuestionId, out var question))
                {
                    result.IsValid = false;
                    result.Errors.Add(new AnswerValidationError
                    {
                        QuestionId = answerDto.QuestionId,
                        QuestionTitle = "Unknown Question",
                        ErrorMessages = new List<string> { "Question not found" }
                    });
                    continue;
                }
                
                var validationErrors = await ValidateAnswerAsync(answerDto, question);
                if (validationErrors.Count > 0)
                {
                    result.IsValid = false;
                    result.Errors.Add(new AnswerValidationError
                    {
                        QuestionId = question.Id,
                        QuestionTitle = GetQuestionTitle(question, languageContext.Language),
                        ErrorMessages = validationErrors
                    });
                }
            }
            
            if (!result.IsValid)
            {
                return result;
            }
            
            // Create or update answers
            foreach (var answerDto in dto.Answers)
            {
                var question = questionDict[answerDto.QuestionId];
                
                try
                {
                    // Check if answer already exists (update scenario)
                    var existingAnswer = await answerRepository.GetByQuestionAndFormSubmissionAsync(
                        answerDto.QuestionId, 
                        dto.FirmId,
                        dto.CategoryId);
                    
                    if (existingAnswer != null)
                    {
                        await UpdateAnswerAsync(existingAnswer, answerDto, question);
                        result.CreatedAnswerIds.Add(existingAnswer.Id);
                    }
                    else
                    {
                        var answer = await CreateAnswerAsync(answerDto, question, dto.FirmId, dto.CategoryId);
                        result.CreatedAnswerIds.Add(answer.Id);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex, 
                        "Error saving answer for QuestionId: {QuestionId}, FirmId: {FirmId}, CategoryId: {CategoryId}",
                        answerDto.QuestionId,
                        dto.FirmId,
                        dto.CategoryId);
                        
                    result.IsValid = false;
                    result.Errors.Add(new AnswerValidationError
                    {
                        QuestionId = question.Id,
                        QuestionTitle = GetQuestionTitle(question, languageContext.Language),
                        ErrorMessages = new List<string> { "Error saving answer" }
                    });
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex, 
                "Error submitting answers for FirmId: {FirmId}, CategoryId: {CategoryId}",
                dto.FirmId,
                dto.CategoryId);
                
            result.IsValid = false;
            result.Errors.Add(new AnswerValidationError
            {
                QuestionId = Guid.Empty,
                QuestionTitle = "System Error",
                ErrorMessages = new List<string> { "An error occurred while submitting answers" }
            });
            return result;
        }
    }
    
    private async Task<Answer> CreateAnswerAsync(
        SubmitAnswerDto dto, 
        Question question, 
        Guid firmId,
        Guid categoryId)
    {
        var answer = new Answer
        {
            QuestionId = dto.QuestionId,
            FirmId = firmId,
            CategoryId = categoryId,
            StructuredData = string.IsNullOrEmpty(dto.StructuredData) 
                ? null 
                : JsonDocument.Parse(dto.StructuredData)
        };
        
        switch (question.Type)
        {
            case QuestionType.String:
            case QuestionType.Text:
                if (!string.IsNullOrWhiteSpace(dto.Value))
                {
                    await AddStringAnswerTranslationsAsync(answer, dto.Value, languageContext.Language);
                }
                break;
                
            case QuestionType.Number:
            case QuestionType.Date:
                answer.Value = dto.Value;
                break;
                
            case QuestionType.SingleSelect:
            case QuestionType.MultiSelect:
                var now = DateTime.UtcNow;
                foreach (var optionId in dto.SelectedOptionIds)
                {
                    answer.SelectedOptions.Add(new AnswerOption
                    {
                        Answer = answer,
                        OptionId = optionId,
                        SelectedAt = now
                    });
                }
                break;
                
            default:
                throw new NotSupportedException($"Unsupported question type: {question.Type}");
        }
        
        return await answerRepository.CreateAsync(answer);
    }
    
    private async Task UpdateAnswerAsync(Answer answer, SubmitAnswerDto dto, Question question)
    {
        answer.StructuredData = string.IsNullOrEmpty(dto.StructuredData) 
            ? null 
            : JsonDocument.Parse(dto.StructuredData);
        
        answer.NeedsUpdating = false; // Reset flag when answer is updated
        
        switch (question.Type)
        {
            case QuestionType.String:
            case QuestionType.Text:
                answer.Translations.Clear();
                if (!string.IsNullOrWhiteSpace(dto.Value))
                {
                    await AddStringAnswerTranslationsAsync(answer, dto.Value, languageContext.Language);
                }
                break;
                
            case QuestionType.Number:
            case QuestionType.Date:
                answer.Value = dto.Value;
                break;
                
            case QuestionType.SingleSelect:
            case QuestionType.MultiSelect:
                answer.SelectedOptions.Clear();
                var now = DateTime.UtcNow;
                foreach (var optionId in dto.SelectedOptionIds)
                {
                    answer.SelectedOptions.Add(new AnswerOption
                    {
                        Answer = answer,
                        OptionId = optionId,
                        SelectedAt = now
                    });
                }
                break;
                
            default:
                throw new NotSupportedException($"Unsupported question type: {question.Type}");
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
            
            try
            {
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
            catch (Exception ex)
            {
                logger.LogError(
                    ex, 
                    "Error translating answer value to {LanguageCode}", 
                    languageCode);
                    
                // Fallback to original value if translation fails
                answer.Translations.Add(new AnswerTranslation
                {
                    Answer = answer,
                    LanguageCode = languageCode,
                    Value = value
                });
            }
        }
    }
    
    public async Task<List<AnswerDto>> GetAnswersAsync(Guid firmId, Guid categoryId, string languageCode)
    {
        try
        {
            var answers = await answerRepository.GetByFormSubmissionAsync(firmId, categoryId);
            
            if (!answers.Any())
            {
                return new List<AnswerDto>();
            }
            
            var questionIds = answers.Select(a => a.QuestionId).Distinct().ToList();
            var questions = await questionRepository.GetByIdsAsync(questionIds);
            var questionDict = questions.ToDictionary(q => q.Id);
            
            return answers
                .Where(a => questionDict.ContainsKey(a.QuestionId))
                .Select(a => MapToDto(a, questionDict[a.QuestionId], languageCode))
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex, 
                "Error getting answers for FirmId: {FirmId}, CategoryId: {CategoryId}",
                firmId,
                categoryId);
            throw;
        }
    }
    
    public async Task<AnswerDto?> GetAnswerAsync(Guid answerId, string languageCode)
    {
        try
        {
            var answer = await answerRepository.GetByIdAsync(answerId);
            if (answer == null)
            {
                return null;
            }
            
            var question = await questionRepository.GetByIdAsync(answer.QuestionId);
            if (question == null)
            {
                logger.LogWarning(
                    "Question not found for Answer: {AnswerId}, QuestionId: {QuestionId}",
                    answerId,
                    answer.QuestionId);
                return null;
            }
            
            return MapToDto(answer, question, languageCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting answer: {AnswerId}", answerId);
            throw;
        }
    }
    
    public async Task<bool> DeleteAnswerAsync(Guid answerId)
    {
        try
        {
            var answer = await answerRepository.GetByIdAsync(answerId);
            if (answer == null)
            {
                return false;
            }
            
            await answerRepository.DeleteAsync(answerId);
            logger.LogInformation("Deleted answer: {AnswerId}", answerId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting answer: {AnswerId}", answerId);
            throw;
        }
    }
    
    public async Task<bool> DeleteAnswersForFormSubmissionAsync(Guid firmId, Guid categoryId)
    {
        try
        {
            var answers = await answerRepository.GetByFormSubmissionAsync(firmId, categoryId);
            if (!answers.Any())
            {
                return false;
            }
            
            var answerIds = answers.Select(a => a.Id).ToList();
            await answerRepository.DeleteRangeAsync(answerIds);
            
            logger.LogInformation(
                "Deleted {Count} answers for FirmId: {FirmId}, CategoryId: {CategoryId}",
                answerIds.Count,
                firmId,
                categoryId);
                
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex, 
                "Error deleting answers for FirmId: {FirmId}, CategoryId: {CategoryId}",
                firmId,
                categoryId);
            throw;
        }
    }
    
    private AnswerDto MapToDto(Answer answer, Question question, string languageCode)
    {
        var dto = new AnswerDto
        {
            Id = answer.Id,
            QuestionId = answer.QuestionId,
            QuestionType = question.Type,
            FirmId = answer.FirmId,
            CategoryId = answer.CategoryId,
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
                dto.SelectedOptions = answer.SelectedOptions
                    .Select(ao => new SelectedOptionDto
                    {
                        OptionId = ao.OptionId,
                        Value = ao.Option.Value,
                        Label = GetOptionLabel(ao.Option, languageCode),
                        SelectedAt = ao.SelectedAt
                    })
                    .OrderBy(o => o.Label)
                    .ToList();
                break;
                
            default:
                logger.LogWarning(
                    "Unsupported question type: {QuestionType} for Question: {QuestionId}",
                    question.Type,
                    question.Id);
                break;
        }
        
        return dto;
    }
    
    private Task<List<string>> ValidateAnswerAsync(SubmitAnswerDto dto, Question question)
    {
        var errors = new List<string>();
        
        // Required field validation
        if (question.IsRequired)
        {
            var hasValue = question.Type switch
            {
                QuestionType.SingleSelect or QuestionType.MultiSelect => 
                    dto.SelectedOptionIds.Count > 0,
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
        
        // Schema validation
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
                    if (!string.IsNullOrWhiteSpace(dto.Value))
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
                    }
                    break;
                }
                    
                case QuestionType.SingleSelect:
                case QuestionType.MultiSelect:
                {
                    // Validate against schema
                    var optionIdsJson = JsonSerializer.Serialize(dto.SelectedOptionIds);
                    var evaluation = schema.Evaluate(optionIdsJson);
                    if (!evaluation.IsValid)
                    {
                        errors.Add(translationService.Translate(
                            ValidationErrorKeys.InvalidOption,
                            languageContext.Language));
                    }
                        
                    // Validate that selected options exist
                    var validOptionIds = question.Options.Select(o => o.Id).ToHashSet();
                    var invalidOptions = dto.SelectedOptionIds
                        .Where(id => !validOptionIds.Contains(id))
                        .ToList();
                            
                    if (invalidOptions.Any())
                    {
                        errors.Add(translationService.Translate(
                            ValidationErrorKeys.InvalidOption,
                            languageContext.Language));
                    }
                        
                    // Validate single select only has one option
                    if (question.Type == QuestionType.SingleSelect && dto.SelectedOptionIds.Count > 1)
                    {
                        errors.Add(translationService.Translate(
                            ValidationErrorKeys.SingleSelectMultipleOptions,
                            languageContext.Language));
                    }
                    break;
                }
                    
                default:
                    throw new NotSupportedException($"Unsupported question type: {question.Type}");
            }
        }
        catch (NotSupportedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex, 
                "Error validating answer for question {QuestionId}", 
                question.Id);
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