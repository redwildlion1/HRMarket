// HRMarket/Core/Answers/AnswerDTOs.cs
using System.Text.Json.Serialization;
using HRMarket.Configuration.Types;

namespace HRMarket.Core.Answers;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(BasicAnswerDto), nameof(QuestionType.String))]
[JsonDerivedType(typeof(BasicAnswerDto), nameof(QuestionType.Text))]
[JsonDerivedType(typeof(BasicAnswerDto), nameof(QuestionType.Number))]
[JsonDerivedType(typeof(BasicAnswerDto), nameof(QuestionType.Date))]
[JsonDerivedType(typeof(SingleChoiceAnswerDto), nameof(QuestionType.SingleSelect))]
[JsonDerivedType(typeof(MultiChoiceAnswerDto), nameof(QuestionType.MultiSelect))]
public abstract class AnswerDto : BaseDto
{
    public Guid QuestionId { get; set; }

    protected AnswerDto()
    {
    }

    protected AnswerDto(Guid questionId)
    {
        QuestionId = questionId;
    }
}

public class BasicAnswerDto : AnswerDto
{
    public string Response { get; set; } = string.Empty;

    public BasicAnswerDto()
    {
    }

    public BasicAnswerDto(Guid questionId, string response) 
        : base(questionId)
    {
        Response = response;
    }
}

public class SingleChoiceAnswerDto : AnswerDto
{
    public Guid SelectedOption { get; set; }

    public SingleChoiceAnswerDto()
    {
    }

    public SingleChoiceAnswerDto(Guid questionId, Guid selectedOption)
        : base(questionId)
    {
        SelectedOption = selectedOption;
    }
}

public class MultiChoiceAnswerDto : AnswerDto
{
    public ICollection<Guid> SelectedOptions { get; set; } = new List<Guid>();

    public MultiChoiceAnswerDto()
    {
    }

    public MultiChoiceAnswerDto(Guid questionId, ICollection<Guid> selectedOptions)
        : base(questionId)
    {
        SelectedOptions = selectedOptions;
    }
}

public class CheckAnswersResult
{
    public bool IsComplete { get; set; }

    public CheckAnswersResult(bool isComplete = true)
    {
        IsComplete = isComplete;
    }
}