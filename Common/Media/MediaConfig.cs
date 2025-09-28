namespace Common.Media;

public class MediaConfig
{
    public string TempBucket { get; set; } = null!;

    public string PermanentBucket { get; set; } = null!;

    public static string FormatTempKey(string fileName, Guid id) => $"temp/{id}-{fileName}";

    public static string FormatPermanentKey(string fileName, Guid id) => $"permanent/{id}-{fileName}";
    public string FormatTempUrl(string fileName, Guid id) => $"https://{TempBucket}.s3.amazonaws.com/{FormatTempKey(fileName, id)}";
    public static (string bucket, string key) ParseS3Url(string s3Url)
    {
        var uri = new Uri(s3Url);
        var hostParts = uri.Host.Split('.');
        var bucket = hostParts[0];
        var key = uri.AbsolutePath.TrimStart('/');
        return (bucket, key);
    }
}