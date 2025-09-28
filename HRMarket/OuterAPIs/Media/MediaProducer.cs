using MassTransit;

namespace HRMarket.OuterAPIs.Media;

public interface IMediaProducer
{
    Task ProduceFileUploaded(Guid fileId, string tempS3Url, Guid userId);
}

public class MediaProducer(IPublishEndpoint publisher) : IMediaProducer
{
    public async Task ProduceFileUploaded(Guid fileId, string tempS3Url, Guid userId)
    {
        await publisher.Publish(new Common.Media.FileUploaded
        {
            FileId = fileId,
            TempS3Url = tempS3Url,
            UserId = userId
        });
    }
}