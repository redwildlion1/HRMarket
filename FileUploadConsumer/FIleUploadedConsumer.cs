using Common.Hub;
using Common.Media;
using FileUploadConsumer.Antivirus;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace FileUploadConsumer;

public class FileUploadedConsumer(
    IClamScanner clamScanner,
    IHubContext<NotificationHub> hubContext,
    IMediaService mediaService)
    : IConsumer<FileUploaded>
{
    public async Task Consume(ConsumeContext<FileUploaded> context)
    {
        var file = await mediaService.DownloadFileAsync(context.Message.TempS3Url);
        var result = await clamScanner.ScanStreamAsync(file);

        if (result.InfectedFiles is not null && result.InfectedFiles.Count > 0)
        {
            await mediaService.DeleteFileAsync(context.Message.TempS3Url);
            // Notify user about the infected file
            await hubContext.Clients.User(context.Message.UserId.ToString())
                .SendAsync("FileScanResult", new
                {
                    context.Message.FileId,
                    Status = "Infected",
                    Details = result.InfectedFiles
                }); 
        }
        else
        {
            // Move the file from temp to permanent storage
            await mediaService.MoveTemporaryFileToPermanentAsync(context.Message.TempS3Url);
            
            // Notify user about the clean file
            await hubContext.Clients.User(context.Message.UserId.ToString())
                .SendAsync("FileScanResult", new
                {
                    context.Message.FileId,
                    Status = "Clean"
                });
        }
    }
}
