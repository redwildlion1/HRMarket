namespace HRMarket.Core.Firms.DTOs;

public class FirmLinksDTO
(string? website, string? linkedIn, string? facebook,
    string? twitter, string? instagram) 
{
    private string Website { get; } = website ?? "";
    private string LinkedIn { get; } = linkedIn ?? "";
    private string Facebook { get; } = facebook ?? "";
    private string Twitter { get; } = twitter ?? "";
    private string Instagram { get; } = instagram ?? "";
    
}