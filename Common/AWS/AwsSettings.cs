namespace Common.AWS;

public class AwsSettings
{
    public const string Section = "AwsSettings";
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string Region { get; set; } = null!;
    public string TempBucketArn { get; set; } = null!;
    public string TempBucketName { get; set; } = null!;
    public string PermanentBucketArn { get; set; } = null!;
    public string PermanentBucketName { get; set; } = null!;
}