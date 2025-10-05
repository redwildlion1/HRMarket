using Amazon.S3;
using Amazon.S3.Model;

namespace Common.Media;

public interface IMediaStorageRepo
{
    Task UploadStreamAsync(string bucket, string key, Stream content, string contentType);
    Task CopyObjectAsync(string sourceBucket, string sourceKey, string destBucket, string destKey, S3CannedACL? acl = null);
    Task DeleteObjectAsync(string bucket, string key);
    Task<Stream> DownloadAsync(string bucket, string key); // caller must dispose
}

public class S3MediaStorageRepo(IAmazonS3 s3) : IMediaStorageRepo
{
    public async Task UploadStreamAsync(string bucket, string key, Stream content, string contentType) {
        var req = new PutObjectRequest { BucketName = bucket, Key = key, InputStream = content, ContentType = contentType };
        await s3.PutObjectAsync(req);
    }

    public async Task CopyObjectAsync(string srcBucket, string srcKey, string dstBucket, string dstKey, S3CannedACL? acl = null) {
        var copy = new CopyObjectRequest {
            SourceBucket = srcBucket, SourceKey = srcKey,
            DestinationBucket = dstBucket, DestinationKey = dstKey
        };
        if (acl is not null) 
            copy.CannedACL = acl;
        await s3.CopyObjectAsync(copy);
    }

    public Task DeleteObjectAsync(string bucket, string key) => s3.DeleteObjectAsync(bucket, key);
    public async Task<Stream> DownloadAsync(string bucket, string key) {
        var resp = await s3.GetObjectAsync(bucket, key);
        var ms = new MemoryStream();
        await resp.ResponseStream.CopyToAsync(ms);
        ms.Position = 0;
        return ms;
    }
}