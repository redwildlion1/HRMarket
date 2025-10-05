namespace HRMarket.Entities.Firms;

public class FirmLinks
{
    public string? Website { get; set; }
    public string? LinkedIn { get; set; }
    public string? Facebook { get; set; }
    public string? Twitter { get; set; }
    public string? Instagram { get; set; }
    

    public Guid FirmId { get; set; }

    public Firm? Firm { get; set; }
    
    public FirmLinks()
    {
    }
    
    
}