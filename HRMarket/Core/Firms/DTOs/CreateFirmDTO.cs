namespace HRMarket.Core.Firms.DTOs;

public class CreateFirmDto
{
    public required string Cui { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? Description { get; set; }

    // Contact properties
    public required string ContactEmail { get; set; }
    public string? ContactPhone { get; set; } 

    // Links properties
    public string? LinksWebsite { get; set; }
    public string? LinksLinkedIn { get; set; }
    public string? LinksFacebook { get; set; }
    public string? LinksTwitter { get; set; }
    public string? LinksInstagram { get; set; }

    // Location properties
    public string? LocationAddress { get; set; }
    public int LocationCountryId { get; set; }
    public int LocationCountyId { get; set; }
    public required string LocationCity { get; set; }
    public string? LocationPostalCode { get; set; }
    
}
