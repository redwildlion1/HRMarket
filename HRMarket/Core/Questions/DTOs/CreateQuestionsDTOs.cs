using System.Text.Json.Serialization;
using HRMarket.Configuration.Types;
using HRMarket.Configuration.UniversalExtensions;

namespace HRMarket.Core.Questions.DTOs;

public class CreateQuestionsForCategoryDTO(Guid categoryId, List<PostQuestionDTO> questions)
{
    public Guid CategoryId { get; } = categoryId;
    public List<PostQuestionDTO> Questions { get; } = questions;
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PostBasicQuestionDTO), nameof(QuestionType.String))]
[JsonDerivedType(typeof(PostBasicQuestionDTO), nameof(QuestionType.Text))]
[JsonDerivedType(typeof(PostBasicQuestionDTO), nameof(QuestionType.Number))]
[JsonDerivedType(typeof(PostBasicQuestionDTO), nameof(QuestionType.Date))]
[JsonDerivedType(typeof(PostOptionQuestionDTO), nameof(QuestionType.SingleSelect))]
[JsonDerivedType(typeof(PostOptionQuestionDTO), nameof(QuestionType.MultiSelect))]
public abstract class PostQuestionDTO : IOrderable
{
    public int Order { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public bool IsRequired { get; set; }
    public bool IsFilter { get; set; }

    protected PostQuestionDTO(
        string title,
        string type,
        bool isRequired,
        int order,
        bool isFilter)
    {
        Title = title;
        Type = type;
        IsRequired = isRequired;
        Order = order;
        IsFilter = isFilter;
    }
}


public class PostBasicQuestionDTO : PostQuestionDTO
{
    public PostBasicQuestionDTO(string title,
        string type,
        bool isRequired,
        int order,
        bool isFilter,
        string validationJson) : base(title, type, isRequired, order, isFilter)
    {
        ValidationJson = validationJson;
    }

    public string ValidationJson { get; set; }
}

public class PostOptionQuestionDTO : PostQuestionDTO
{
    public PostOptionQuestionDTO(string title,
        string type,
        bool isRequired,
        int order,
        bool isFilter,
        List<PostOptionDTO> options) : base(title, type, isRequired, order, isFilter)
    {
        Options = options;
    }

    public List<PostOptionDTO> Options { get; set; }
}

public class PostOptionDTO(string text, int order) : IOrderable
{
    public string Text { get; set; } = text;
    public int Order { get; set; } = order;
}