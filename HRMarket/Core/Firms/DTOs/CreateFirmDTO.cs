namespace HRMarket.Core.Firms.DTOs;

public class CreateFirmDTO(
    string cui,
    string firmName,
    string firmType,
    string description,
    FirmContactDTO contact,
    FirmLinksDTO links,
    FirmLocationDTO location,
    List<Guid> categoryIds,
    FormDTO form) 
{
    public string Cui { get; set; } = cui;
    public string FirmName { get; set; } = firmName;
    public string FirmType { get; set; } = firmType;
    public string Description { get; set; } = description;
    public FirmContactDTO Contact { get; set; } = contact;
    public FirmLinksDTO Links { get; set; } = links;
    public FirmLocationDTO Location { get; set; } = location;
    public List<Guid> CategoryIds { get; set; } = categoryIds;
    public FormDTO Form { get; set; } = form;
}