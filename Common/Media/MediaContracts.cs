namespace Common.Media;

public record FileUploaded
{
    public Guid FileId { get; set; }
    public required string TempS3Url { get; set; }
    public Guid UserId { get; set; }
}