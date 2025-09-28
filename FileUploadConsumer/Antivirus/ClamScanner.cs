using nClam;

namespace FileUploadConsumer.Antivirus;

public interface IClamScanner { Task<ClamScanResult> ScanStreamAsync(Stream stream, string? suggestedFileName = null); }

public class ClamScanner(string host = "127.0.0.1", int port = 3310, long maxStreamSize = 50L * 1024 * 1024) : IClamScanner
{
    private readonly ClamClient _clam = new(host, port)
    {
        // keep MaxStreamSize in-sync with clamd.conf StreamMaxLength
        MaxStreamSize = maxStreamSize
    };

    // returns the raw scan result (you can map to your domain)
    public async Task<ClamScanResult> ScanStreamAsync(Stream incoming, string? suggestedFileName = null)
    {
        // ensure stream is readable; if not, copy to memory/temp first
        if (!incoming.CanRead)
            throw new ArgumentException("Incoming stream must be readable", nameof(incoming));

        // if the stream is seekable, reset position to 0 for safe reuse
        if (incoming.CanSeek)
            incoming.Position = 0;

        try
        {
            // Preferred: stream directly via INSTREAM
            var result = await _clam.SendAndScanFileAsync(incoming);
            return result;
        }
        catch (Exception ex)
        {
            // look for INSTREAM/size-limit type errors in the message (clamd returns text)
            var msg = ex.Message;
            if (!msg.Contains("INSTREAM size limit") && !msg.Contains("size limit exceeded") &&
                !msg.Contains("StreamMaxLength")) throw;
            // fallback: save stream to temp file then call ScanFileOnServerAsync
            var tempPath = Path.Combine(Path.GetTempPath(), $"clamtmp-{Guid.NewGuid():N}{Path.GetExtension(suggestedFileName ?? ".bin")}");
            try
            {
                // make sure we copy from start
                if (incoming.CanSeek) incoming.Position = 0;
                await using (var fs = File.Create(tempPath))
                {
                    await incoming.CopyToAsync(fs);
                }

                // Ask clamd to scan the file path (works when clamd can access the path)
                var serverResult = await _clam.ScanFileOnServerAsync(tempPath);
                return serverResult;
            }
            finally
            {
                try { File.Delete(tempPath); } catch { /* swallow cleanup errors */ }
            }
        }
    }
}



