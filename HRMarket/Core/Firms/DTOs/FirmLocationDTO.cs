namespace HRMarket.Core.Firms.DTOs;

public class FirmLocationDTO(
    string address,
    Guid countryId,
    Guid countyId,
    Guid cityId) 
{
    private string Address { get; set; } = address;
    private Guid CountryId { get; set; } = countryId;
    private Guid CountyId { get; set; } = countyId;
    private Guid CityId { get; set; } = cityId;
}