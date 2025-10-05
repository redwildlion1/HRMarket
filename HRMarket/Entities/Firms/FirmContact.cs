namespace HRMarket.Entities.Firms;

public class FirmContact
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    

    public Guid FirmId { get; init; } 

    public Firm? Firm { get; init; }

    public FirmContact()
    {
    }

}