using System.Text.Json.Serialization;
using HRMarket.Configuration.Types;

namespace HRMarket.Core.Answers;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(BasicAnswerDTO), nameof(QuestionType.String))]
[JsonDerivedType(typeof(BasicAnswerDTO), nameof(QuestionType.Text))]
[JsonDerivedType(typeof(BasicAnswerDTO), nameof(QuestionType.Number))]
[JsonDerivedType(typeof(BasicAnswerDTO), nameof(QuestionType.Date))]
[JsonDerivedType(typeof(SingleChoiceAnswerDTO), nameof(QuestionType.SingleSelect))]
[JsonDerivedType(typeof(MultiChoiceAnswerDTO), nameof(QuestionType.MultiSelect))]
public abstract class AnswerDTO
{
    public Guid QuestionId { get; set; }

    protected AnswerDTO(Guid questionId)
    {
        QuestionId = questionId;
    }
}

// for String, Text, Number, Date
public class BasicAnswerDTO : AnswerDTO
{
    public string Response { get; set; }

    public BasicAnswerDTO(Guid questionId, string response) 
        : base(questionId)
    {
        Response = response;
    }
}

// for SingleSelect
public class SingleChoiceAnswerDTO : AnswerDTO
{
    public Guid SelectedOption { get; set; }

    public SingleChoiceAnswerDTO(Guid questionId, Guid selectedOption)
        : base(questionId)
    {
        SelectedOption = selectedOption;
    }
}

// for MultiSelect
public class MultiChoiceAnswerDTO : AnswerDTO
{
    public ICollection<Guid> SelectedOptions { get; set; }

    public MultiChoiceAnswerDTO(Guid questionId, ICollection<Guid> selectedOptions)
        : base(questionId)
    {
        SelectedOptions = selectedOptions;
    }
}

// result object
public class CheckAnswersResult
{
    public bool IsComplete { get; set; }

    public CheckAnswersResult(bool isComplete = true)
    {
        IsComplete = isComplete;
    }
}