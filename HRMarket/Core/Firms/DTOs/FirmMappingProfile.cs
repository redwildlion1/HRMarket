using AutoMapper;
using HRMarket.Entities.Firms;

namespace HRMarket.Core.Firms.DTOs;

public class FirmMappingProfile : Profile
{
    public FirmMappingProfile()
    {
        CreateMap<CreateFirmDTO, Firm>()
            .ForMember(dest => dest.Contact, opt => opt.MapFrom(src => src.Contact))
            .ForMember(dest => dest.Links, opt => opt.MapFrom(src => src.Links))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.CategoryIds, opt => opt.MapFrom(src => src.CategoryIds));

        CreateMap<FirmContactDTO, FirmContact>();
        CreateMap<FirmLinksDTO, FirmLinks>();
        CreateMap<FirmLocationDTO, FirmLocation>();
    }
}