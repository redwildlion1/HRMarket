// HRMarket/Core/Answers/SubmitAnswersDto.cs

using HRMarket.Configuration.Types;

namespace HRMarket.Core.Answers;

public class SubmitAnswersDto
{
    public Guid FirmId { get; set; }
    public Guid CategoryId { get; set; }
    public List<SubmitAnswerDto> Answers { get; set; } = new();
}

public class SubmitAnswerDto
{
    public Guid QuestionId { get; set; }
    public string? Value { get; set; }
    public List<Guid> SelectedOptionIds { get; set; } = new();
    public string? StructuredData { get; set; }
}

public class SubmitAnswersResultDto
{
    public bool IsValid { get; set; }
    public List<Guid> CreatedAnswerIds { get; set; } = new();
    public List<AnswerValidationError> Errors { get; set; } = new();
}

public class AnswerValidationError
{
    public Guid QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public List<string> ErrorMessages { get; set; } = new();
}

public class AnswerDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }
    public Guid FirmId { get; set; }
    public Guid CategoryId { get; set; }
    public string? Value { get; set; }
    public List<SelectedOptionDto> SelectedOptions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool NeedsUpdating { get; set; }
}

public class SelectedOptionDto
{
    public Guid OptionId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DateTime SelectedAt { get; set; }
}