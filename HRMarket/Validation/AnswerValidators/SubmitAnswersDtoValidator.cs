// HRMarket/Core/Answers/Validators/SubmitAnswersDtoValidator.cs

using FluentValidation;
using HRMarket.Core.Answers;

namespace HRMarket.Validation.AnswerValidators;

public class SubmitAnswersDtoValidator : AbstractValidator<SubmitAnswersDto>
{
    public SubmitAnswersDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Category ID is required");
        
        RuleFor(x => x.Answers)
            .NotEmpty()
            .WithMessage("At least one answer is required");
        
        RuleForEach(x => x.Answers)
            .SetValidator(new SubmitAnswerDtoValidator());
    }
}

public class SubmitAnswerDtoValidator : AbstractValidator<SubmitAnswerDto>
{
    public SubmitAnswerDtoValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage("Question ID is required");
        
        RuleFor(x => x.StructuredData)
            .Must(BeValidJsonOrNull)
            .When(x => !string.IsNullOrEmpty(x.StructuredData))
            .WithMessage("Structured data must be valid JSON");
    }
    
    private static bool BeValidJsonOrNull(string? json)
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