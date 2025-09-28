using Common.Media;
using HRMarket.Configuration.Status;
using HRMarket.Configuration.Types;
using HRMarket.Entities;
using HRMarket.Entities.Medias;
using HRMarket.OuterAPIs.Media;

namespace HRMarket.Core.Media;

public class FileUploadBuilder(
    IMediaService mediaService,
    ApplicationDbContext db,
    IMediaProducer publisher,
    IFormFile file,
    Guid userId)
{
    private FirmMediaDetails? _firmMediaDetails;
    private bool _saveTempAndScan;
    private List<FileType>? _allowedTypes;

    public FileUploadBuilder ForFirm(Guid firmId, FirmMediaType type)
    {
        _firmMediaDetails = new FirmMediaDetails(firmId, type);
        return this;
    }

    public FileUploadBuilder SaveTempAndScan()
    {
        _saveTempAndScan = true;
        return this;
    }

    public FileUploadBuilder WithAllowedTypes(List<FileType> types)
    {
        _allowedTypes = types;
        return this;
    }

    public async Task<MediaStatus> SaveAsync()
    {
        var status = MediaStatus.Scanning;
        var fileType = file.IsOfType();
        
        if (_allowedTypes != null)
        {
            CheckAllowedTypes(fileType, _allowedTypes);
        }
        
        var media = new Entities.Medias.Media
        {
            OriginalFileName = file.FileName,
            SizeInBytes = file.Length,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            Height = null,
            Width = null
        };
        
        Guid fileId;
        
        if (_firmMediaDetails != null)
        {
            var firmMedia = new FirmMedia
            {
                FirmId = _firmMediaDetails.FirmId,
                FirmMediaType = _firmMediaDetails.MediaType,
                Media = media
            };
           fileId = db.Add(firmMedia).Entity.MediaId;
        }
        else
        {
           fileId =  db.Add(media).Entity.Id;
        }

        await db.SaveChangesAsync();

        if (_saveTempAndScan)
        {
           var tempUrl = await SaveTempAndPublish(file, userId, fileId);
           media.S3KeyTemp = tempUrl;
           media.Status = MediaStatus.Scanning;
        }
        else
        {
            // Directly save to permanent storage (not recommended without scanning)
            var permUrl = await mediaService.UploadPermanentFileAsync(file, fileId);
            media.S3KeyFinal = permUrl;
            media.Status = MediaStatus.Available;
            status = MediaStatus.Available;
        }

        await db.SaveChangesAsync();
        return status;
    }
    
    private static void CheckAllowedTypes(FileType type, List<FileType> types)
    {
        if (!types.Contains(type))
        {
            throw new InvalidOperationException($"File type {type} is not allowed.");
        }
    }
    
    private async Task<string> SaveTempAndPublish(IFormFile fileIn, Guid userIdIn, Guid fileId)
    {
        // save to temp storage
       var tempS3Url = await mediaService.UploadTemporaryFileAsync(fileIn, fileId);
        
       await publisher.ProduceFileUploaded(fileId, tempS3Url, userIdIn);
       
        return tempS3Url;
    }
}

internal record FirmMediaDetails(Guid FirmId, FirmMediaType MediaType);

