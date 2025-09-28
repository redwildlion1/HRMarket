namespace HRMarket.Configuration.Types;

public enum FileType
{
    Pdf,
    Jpeg,
    Jpg,
    Png,
}

public static class FileTypeExtensions
{
    private static readonly Dictionary<FileType, byte[]> MagicBytes = new()
    {
        { FileType.Pdf, "%PDF"u8.ToArray() }, // %PDF
        { FileType.Jpeg, [0xFF, 0xD8, 0xFF] },
        { FileType.Png, [0x89, 0x50, 0x4E, 0x47] },
        { FileType.Jpg, [0xFF, 0xD8, 0xFF]}
    };
    
    private static readonly Dictionary<FileType, string> FileExtensions = new()
    {
        { FileType.Pdf, ".pdf" },
        { FileType.Jpeg, ".jpeg" },
        { FileType.Png, ".png" },
        { FileType.Jpg, ".jpg" }
    };
    
    public static FileType IsOfType(this IFormFile s)
    {
        var ext = Path.GetExtension(s.FileName).ToLower();
        var expectedExtensions = FileExtensions.Values;
        if (!expectedExtensions.Contains(ext)) 
            throw new InvalidOperationException($"File extension {ext} not allowed." +
                                                $" Allowed: {string.Join(", ", expectedExtensions)}");

        using var ms = new MemoryStream();
        s.CopyTo(ms);
        ms.Position = 0;
        Span<byte> header = stackalloc byte[8];
        ms.ReadExactly(header);
        ms.Position = 0;

        foreach (var magic in FileExtensions.Select(type => MagicBytes[type.Key]))
        {
            if (header[..magic.Length].SequenceEqual(magic))
            {
                return FileExtensions.First(t => MagicBytes[t.Key] == magic).Key;
            }
        }

        throw new InvalidOperationException("File type not allowed based on magic bytes.");
    }

}