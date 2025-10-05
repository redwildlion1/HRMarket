using Common.AWS;

namespace Common.Media;

public class AwsConfigurator(AwsSettings awsSettings)
{
    public string TempBucket => awsSettings.TempBucketName;
    public string PermanentBucket => awsSettings.PermanentBucketName;
    public static string FormatKey(string fileName, Guid id) => $"{id}-{fileName}";
    public string FormatTempUrl(string fileName, Guid id) => $"https://{TempBucket}.s3.amazonaws.com/{FormatKey(fileName, id)}";
    public string FormatPermanentUrl(string fileName, Guid id) => $"https://{PermanentBucket}.s3.amazonaws.com/{FormatKey(fileName, id)}";
    public static (string bucket, string key) ParseS3Url(string s3Url)
    {
        var uri = new Uri(s3Url);
        var hostParts = uri.Host.Split('.');
        var bucket = hostParts[0];
        var key = uri.AbsolutePath.TrimStart('/');
        return (bucket, key);
    }
}