using FluentValidation;
using HRMarket.Configuration;
using HRMarket.Configuration.Translation;
using HRMarket.Configuration.Types;
using HRMarket.Core.Questions.DTOs;

namespace HRMarket.Validation.QuestionValidators;

public class CreateQuestionDtoValidator : AbstractValidator<CreateQuestionDto>
{
    public CreateQuestionDtoValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid question type");
        
        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Order must be non-negative");
        
        RuleFor(x => x.ValidationSchema)
            .NotEmpty()
            .WithMessage("Validation schema is required")
            .Must(BeValidJson)
            .WithMessage("Validation schema must be valid JSON");
        
        RuleFor(x => x.Translations)
            .NotEmpty()
            .WithMessage("At least one translation is required")
            .Must(HaveAllSupportedLanguages)
            .WithMessage($"Translations for all supported languages are required: {string.Join(", ", SupportedLanguages.All)}");
        
        RuleForEach(x => x.Translations)
            .SetValidator(new QuestionTranslationDtoValidator());
        
        RuleFor(x => x.Options)
            .NotEmpty()
            .When(x => x.Type == QuestionType.SingleSelect || x.Type == QuestionType.MultiSelect)
            .WithMessage("Options are required for choice-based questions");
        
        RuleFor(x => x.Options)
            .Empty()
            .When(x => x.Type != QuestionType.SingleSelect && x.Type != QuestionType.MultiSelect)
            .WithMessage("Options should only be provided for choice-based questions");
        
        RuleForEach(x => x.Options)
            .SetValidator(new CreateQuestionOptionDtoValidator());
    }
    
    private bool BeValidJson(string json)
    {
        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private bool HaveAllSupportedLanguages(List<QuestionTranslationDto> translations)
    {
        var providedLanguages = translations.Select(t => t.LanguageCode.ToLower()).ToHashSet();
        return SupportedLanguages.All.All(lang => providedLanguages.Contains(lang));
    }
}

public class QuestionTranslationDtoValidator : AbstractValidator<QuestionTranslationDto>
{
    public QuestionTranslationDtoValidator()
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .WithMessage("Language code is required")
            .Must(SupportedLanguages.IsSupported)
            .WithMessage($"Language must be one of: {string.Join(", ", SupportedLanguages.All)}");
        
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(AppConstants.MaxQuestionTitleLength)
            .WithMessage($"Title must not exceed {AppConstants.MaxQuestionTitleLength} characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(AppConstants.MaxDescriptionLength)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage($"Description must not exceed {AppConstants.MaxDescriptionLength} characters");
        
        RuleFor(x => x.Placeholder)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Placeholder))
            .WithMessage("Placeholder must not exceed 200 characters");
    }
}

public class CreateQuestionOptionDtoValidator : AbstractValidator<CreateQuestionOptionDto>
{
    public CreateQuestionOptionDtoValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("Option value is required")
            .MaximumLength(AppConstants.MaxTokenLength)
            .WithMessage($"Value must not exceed {AppConstants.MaxTokenLength} characters");
        
        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Order must be non-negative");
        
        RuleFor(x => x.Translations)
            .NotEmpty()
            .WithMessage("At least one translation is required")
            .Must(HaveAllSupportedLanguages)
            .WithMessage($"Translations for all supported languages are required: {string.Join(", ", SupportedLanguages.All)}");
        
        RuleForEach(x => x.Translations)
            .SetValidator(new OptionTranslationDtoValidator());
        
        RuleFor(x => x.Metadata)
            .Must(BeValidJsonOrNull)
            .When(x => !string.IsNullOrEmpty(x.Metadata))
            .WithMessage("Metadata must be valid JSON");
    }
    
    private bool HaveAllSupportedLanguages(List<OptionTranslationDto> translations)
    {
        var providedLanguages = translations.Select(t => t.LanguageCode.ToLower()).ToHashSet();
        return SupportedLanguages.All.All(lang => providedLanguages.Contains(lang));
    }
    
    private bool BeValidJsonOrNull(string? json)
    {
        if (string.IsNullOrEmpty(json)) return true;
        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class OptionTranslationDtoValidator : AbstractValidator<OptionTranslationDto>
{
    public OptionTranslationDtoValidator()
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .WithMessage("Language code is required")
            .Must(SupportedLanguages.IsSupported)
            .WithMessage($"Language must be one of: {string.Join(", ", SupportedLanguages.All)}");
        
        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Label is required")
            .MaximumLength(AppConstants.MaxOptionTextLength)
            .WithMessage($"Label must not exceed {AppConstants.MaxOptionTextLength} characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(300)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 300 characters");
    }
}