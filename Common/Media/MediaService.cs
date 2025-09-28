using Microsoft.AspNetCore.Http;

namespace Common.Media;

public interface IMediaService
{
    Task <string> UploadTemporaryFileAsync(IFormFile file, Guid fileId);
    Task <string> UploadPermanentFileAsync(IFormFile file, Guid fileId);
    Task<Stream> DownloadFileAsync(string fileUrl);
    Task MoveTemporaryFileToPermanentAsync(string tempFileUrl);
    Task DeleteFileAsync(string fileUrl);
}

public class MediaService(IMediaStorageRepo repo, MediaConfig config) : IMediaService
{
    public async Task<string> UploadTemporaryFileAsync(IFormFile file, Guid fileId)
    {
        
        await repo.UploadStreamAsync(
            config.TempBucket,
            MediaConfig.FormatTempKey(file.FileName, fileId),
            file.OpenReadStream(),
            file.ContentType);
        
        //This should return the fileId and the url of the uploaded file
        return config.FormatTempUrl(file.FileName, fileId); 
    }

    public async Task<string> UploadPermanentFileAsync(IFormFile file, Guid fileId)
    {
        await repo.UploadStreamAsync(
            config.PermanentBucket,
            MediaConfig.FormatPermanentKey(file.FileName, fileId),
            file.OpenReadStream(),
            file.ContentType);
        
        return $"https://{config.PermanentBucket}.s3.amazonaws.com/{MediaConfig.FormatPermanentKey(file.FileName, fileId)}";
    }

    public async Task<Stream> DownloadFileAsync(string fileUrl)
    {
        var (bucket, key) = MediaConfig.ParseS3Url(fileUrl);
        return await repo.DownloadAsync(bucket, key);
    }
    
    public async Task MoveTemporaryFileToPermanentAsync(string tempFileUrl)
    {
        var (bucket, key) = MediaConfig.ParseS3Url(tempFileUrl);
        var permanentKey = key.Replace("temp/", "permanent/");
        
        await repo.CopyObjectAsync(bucket, key, bucket, permanentKey);
        await repo.DeleteObjectAsync(bucket, key);
    }
    
    public async Task DeleteFileAsync(string fileUrl)
    {
        var (bucket, key) = MediaConfig.ParseS3Url(fileUrl);
        await repo.DeleteObjectAsync(bucket, key);
    }
}