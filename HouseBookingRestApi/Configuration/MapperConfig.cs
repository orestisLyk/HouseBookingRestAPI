using AutoMapper;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Configuration
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<User, UserReadOnlyDTO>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));

            CreateMap<UserRegisterDTO, User>();

            CreateMap<House, HouseReadOnlyDTO>()
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.HouseImages.Select(i => i.Url).ToList()))
                .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId));

            CreateMap<HouseRegisterDTO, House>();

            CreateMap<HouseUpdateDTO, House>();

            CreateMap<HouseImage, HouseImageReadOnlyDTO>();

            CreateMap<HouseImageCreateDTO, HouseImage>();

            CreateMap<Booking, BookingReadOnlyDTO>()
                .ForMember(dest => dest.HouseName, opt => opt.MapFrom(src => src.House.Name));

            CreateMap<BookingRegisterDTO, Booking>();

            CreateMap<Role, RoleReadOnlyDTO>();

            


        }
    }
}
