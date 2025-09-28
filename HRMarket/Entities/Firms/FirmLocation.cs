using HRMarket.Entities.LocationElements;

namespace HRMarket.Entities.Firms;

public class FirmLocation
{
    public string City { get; set; } = "";
    public string Address { get; set; } = "";
    public string PostalCode { get; set; } = "";
    
    public int CountryId { get; set; }
    public Country? Country { get; set; }
    public int CountyId { get; set; }
    public County? County{ get; set; }
    
    public Firm? Firm { get; set; } = null!;
    public Guid? FirmId { get; set; }
    
}