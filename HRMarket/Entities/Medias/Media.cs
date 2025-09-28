using HRMarket.Configuration.Status;
using HRMarket.Configuration.Types;

namespace HRMarket.Entities.Medias;

public class Media 
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public FileType FileType { get; set; }
    public long SizeInBytes { get; set; }
    public string? S3KeyTemp { get; set; }
    public string? S3KeyFinal { get; set; } 
    public int? Width { get; set; }
    public int? Height { get; set; }
    public MediaStatus Status { get; set; } = MediaStatus.Scanning;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public FirmMedia? FirmMedia { get; set; }
}