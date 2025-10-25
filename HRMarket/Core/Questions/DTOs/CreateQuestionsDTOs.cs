using System.Text.Json;
using HRMarket.Configuration.Types;

namespace HRMarket.Core.Questions.DTOs;

public class CreateQuestionDto : BaseDto
{
    public QuestionType Type { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public bool IsFilter { get; set; }
    public string ValidationSchema { get; set; } = null!;
    public List<QuestionTranslationDto> Translations { get; set; } = [];
    public List<CreateQuestionOptionDto> Options { get; set; } = [];
}

public class UpdateQuestionDto : BaseDto
{
    public Guid Id { get; set; }
    public QuestionType Type { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public bool IsFilter { get; set; }
    public string ValidationSchema { get; set; } = null!;
    public List<QuestionTranslationDto> Translations { get; set; } = new();
    public List<CreateQuestionOptionDto> Options { get; set; } = new();
}

public class QuestionTranslationDto
{
    public string LanguageCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Placeholder { get; set; }
}

public class CreateQuestionOptionDto
{
    public string Value { get; set; } = null!;
    public int Order { get; set; }
    public List<OptionTranslationDto> Translations { get; set; } = new();
    public string? Metadata { get; set; }
}

public class OptionTranslationDto
{
    public string LanguageCode { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string? Description { get; set; }
}

public class BulkCreateQuestionsDto : BaseDto
{
    public Guid CategoryId { get; set; }
    public List<CreateQuestionDto> Questions { get; set; } = new();
}

public class QuestionDto
{
    public Guid Id { get; set; }
    public QuestionType Type { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Placeholder { get; set; }
    public JsonDocument ValidationSchema { get; set; } = null!;
    public List<QuestionOptionDto> Options { get; set; } = new();
}

public class QuestionOptionDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = null!;
    public int Order { get; set; }
    public string Label { get; set; } = null!;
    public string? Description { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class QuestionListDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public List<QuestionDto> Questions { get; set; } = new();
}