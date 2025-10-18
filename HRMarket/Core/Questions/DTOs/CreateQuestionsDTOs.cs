using System.Text.Json;
using System.Text.Json.Serialization;
using HRMarket.Configuration.Types;
using HRMarket.Configuration.UniversalExtensions;

namespace HRMarket.Core.Questions.DTOs;

public class CreateQuestionsForCategoryDto(Guid categoryId, List<PostQuestionDto> questions)
{
    public Guid CategoryId { get; } = categoryId;
    public List<PostQuestionDto> Questions { get; } = questions;
}


public class PostQuestionDto : IOrderable
{
    public int Order { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public bool IsRequired { get; set; }
    public bool IsFilter { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ValidationJson { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PostOptionDto>? Options { get; set; }
    public List<PostQuestionVariantDto>? Variants { get; set; }

    public PostQuestionDto(
        string title,
        string type,
        bool isRequired,
        int order,
        bool isFilter,
        string? validationJson = null,
        List<PostOptionDto>? options = null)
    {
        Title = title;
        Type = type;
        IsRequired = isRequired;
        Order = order;
        IsFilter = isFilter;
        ValidationJson = validationJson;
        Options = options;
    }
}

public class PostQuestionVariantDto(string languageId, string title)
{
    public string LanguageId { get; set; } = languageId;
    public string Title { get; set; } = title;
}


public class PostOptionDto(string text, int order, List<PostOptionVariantDto> variants) : IOrderable
{
    public string Text { get; set; } = text;
    public int Order { get; set; } = order;
    public List<PostOptionVariantDto>? Variants { get; set; } = variants;
}

public class PostOptionVariantDto(string languageId, string value)
{
    public string LanguageId { get; set; } = languageId;
    public string Value { get; set; } = value;
}
    
    
