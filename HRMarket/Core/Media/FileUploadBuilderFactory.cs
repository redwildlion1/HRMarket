using Common.Media;
using HRMarket.Entities;
using HRMarket.OuterAPIs.Media;
using MassTransit;

namespace HRMarket.Core.Media;

public interface IFileUploadBuilderFactory
{
    FileUploadBuilder Create(IFormFile file, Guid userId);
}

public class FileUploadBuilderFactory(
    IMediaService mediaService,
    ApplicationDbContext db,
    IMediaProducer publisher)
    : IFileUploadBuilderFactory
{
    public FileUploadBuilder Create(IFormFile file, Guid userId)
    {
        return new FileUploadBuilder(mediaService, db, publisher, file, userId);
    }
}
