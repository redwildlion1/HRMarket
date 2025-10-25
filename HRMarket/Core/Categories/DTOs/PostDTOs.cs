namespace HRMarket.Core.Categories.DTOs;

public class TranslationDto
{
    public required string LanguageCode { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class PostClusterDto : BaseDto
{
    public required string Icon { get; set; }
    public required List<TranslationDto> Translations { get; set; } = [];
}

public class PostCategoryDto : BaseDto
{
    public required string Icon { get; set; }
    public int? OrderInCluster { get; set; }
    public Guid? ClusterId { get; set; }
    public required List<TranslationDto> Translations { get; set; } = [];
}

public class PostServiceDto : BaseDto
{
    public int OrderInCategory { get; set; }
    public Guid CategoryId { get; set; }
    public required List<TranslationDto> Translations { get; set; } = [];
}

public class UpdateClusterDto : BaseDto
{
    public required Guid Id { get; set; }
    public required string Icon { get; set; }
    public bool IsActive { get; set; }
    public required List<TranslationDto> Translations { get; set; } = [];
}

public class UpdateCategoryDto : BaseDto
{
    public required Guid Id { get; set; }
    public required string Icon { get; set; }
    public int? OrderInCluster { get; set; }
    public Guid? ClusterId { get; set; }
    public required List<TranslationDto> Translations { get; set; } = [];
}

public class UpdateServiceDto : BaseDto
{
    public required Guid Id { get; set; }
    public int OrderInCategory { get; set; }
    public Guid CategoryId { get; set; }
    public required List<TranslationDto> Translations { get; set; } = [];
}

public class AddCategoryToClusterDto(Guid categoryId, Guid clusterId) : BaseDto
{
    public Guid CategoryId { get; } = categoryId;
    public Guid ClusterId { get; } = clusterId;
}