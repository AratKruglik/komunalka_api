using AutoMapper;
using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;

namespace KomunalkaAPI.Mapping;

/// <summary>
/// AutoMapper profile for mapping models to DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Address mappings
        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region))
            .ForMember(dest => dest.AddressType, opt => opt.MapFrom(src => src.AddressType));

        CreateMap<CreateAddressDto, Address>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Region, opt => opt.Ignore())
            .ForMember(dest => dest.AddressType, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Region mappings
        CreateMap<Region, RegionDto>();

        // AddressType mappings
        CreateMap<AddressType, AddressTypeDto>();

        // Currency mappings
        CreateMap<Currency, CurrencyDto>();
        CreateMap<CreateCurrencyDto, Currency>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateCurrencyDto, Currency>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        // User mappings
        CreateMap<User, UserDto>();
    }
}
