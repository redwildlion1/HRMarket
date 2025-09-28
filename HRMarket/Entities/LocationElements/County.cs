namespace HRMarket.Entities.LocationElements;

public class County
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public int CountryId { get; init; }
    public Country? Country { get; init; }
}