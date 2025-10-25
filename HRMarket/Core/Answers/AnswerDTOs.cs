using HRMarket.Configuration.Types;

namespace HRMarket.Core.Answers;

public class SubmitAnswerDto : BaseDto
{
    public Guid QuestionId { get; set; }
    public string? Value { get; set; }
    public List<Guid> SelectedOptionIds { get; set; } = new();
    public string? StructuredData { get; set; }
}

public class SubmitAnswersDto : BaseDto
{
    public Guid CategoryId { get; set; }
    public List<SubmitAnswerDto> Answers { get; set; } = new();
}

public class AnswerDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }
    public string? Value { get; set; }
    public List<SelectedOptionDto> SelectedOptions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool NeedsUpdating { get; set; }
}

public class SelectedOptionDto
{
    public Guid OptionId { get; set; }
    public string Value { get; set; } = null!;
    public string Label { get; set; } = null!;
    public DateTime SelectedAt { get; set; }
}

public class SubmitAnswersResultDto
{
    public bool IsValid { get; set; }
    public List<AnswerValidationError> Errors { get; set; } = [];
    public List<Guid> CreatedAnswerIds { get; set; } = [];
}

public class AnswerValidationError
{
    public Guid QuestionId { get; set; }
    public string QuestionTitle { get; set; } = null!;
    public List<string> ErrorMessages { get; set; } = [];
}