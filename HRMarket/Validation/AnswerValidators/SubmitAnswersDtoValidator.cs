// HRMarket/Core/Answers/SubmitAnswersDtoValidator.cs
using FluentValidation;

namespace HRMarket.Core.Answers;

public class SubmitAnswersDtoValidator : AbstractValidator<SubmitAnswersDto>
{
    public SubmitAnswersDtoValidator()
    {
        RuleFor(x => x.FirmId)
            .NotEmpty()
            .WithMessage("FirmId is required");
            
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("CategoryId is required");
            
        RuleFor(x => x.Answers)
            .NotNull()
            .WithMessage("Answers collection is required");
            
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
            .WithMessage("QuestionId is required");
            
        // Note: Value and SelectedOptionIds validation is done at business logic level
        // since it depends on question type and requirements
    }
}