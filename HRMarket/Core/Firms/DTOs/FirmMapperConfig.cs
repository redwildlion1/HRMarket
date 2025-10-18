using HRMarket.Configuration.Types;
using HRMarket.Entities.Firms;
using Mapster;

namespace HRMarket.Core.Firms.DTOs;

public class FirmMapperConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<CreateFirmDto, Firm>
            .NewConfig()
            .Map(dest => dest.Type, src => Enum.Parse<FirmType>(src.Type, true))
            .Map(dest => dest.Contact, src => new FirmContact
            {
                Email = src.ContactEmail,
                Phone = src.ContactPhone
            })
            .Map(dest => dest.Links, src => new FirmLinks
            {
                Website = src.LinksWebsite,
                LinkedIn = src.LinksLinkedIn,
                Facebook = src.LinksFacebook,
                Twitter = src.LinksTwitter,
                Instagram = src.LinksInstagram
            })
            .Map(dest => dest.Location, src => new FirmLocation
            {
                Address = src.LocationAddress,
                CountryId = src.LocationCountryId,
                CountyId = src.LocationCountyId,
                City = src.LocationCity,
                PostalCode = src.LocationPostalCode
            });
    }
}