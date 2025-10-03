using AutoMapper;
using Data.Entities;
using Domain.Dtos;
using Domain.Dtos.Payment;
using System.Globalization;

namespace Data.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Room mappings
        CreateMap<RoomEntity, RoomDto>()
            .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => src.SerialNumber.ToUpper()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.StatusName));
        CreateMap<RoomDto, RoomEntity>()
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        // RoomStatus mappings
        CreateMap<RoomStatusEntity, RoomStatusDto>().ReverseMap();

        // Customer mappings
        CreateMap<CustomerEntity, CustomerDto>().ReverseMap();

        // Reservation mappings
        CreateMap<ReservationEntity, ReservationDto>()
            .ForMember(dest => dest.PriceDescription, opt => opt.MapFrom(src => src.PriceDescription ?? src.Price.ToString("C", CultureInfo.CreateSpecificCulture("tr-TR"))));
        CreateMap<ReservationDto, ReservationEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // Payment mappings
        CreateMap<PaymentEntity, PaymentDto>();
        CreateMap<PaymentDto, PaymentEntity>();


        // CustomerBalance mappings
        CreateMap<CustomerBalanceEntity, CustomerBalanceDto>()
            .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.TransactionType.ToString()))
            .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : null));
        CreateMap<CustomerBalanceDto, CustomerBalanceEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? Guid.NewGuid().ToString()))
            .ForMember(dest => dest.Reservation, opt => opt.Ignore())
            .ForMember(dest => dest.Payment, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore());

        // PaymentMethod mappings
        CreateMap<PaymentMethodEntity, PaymentMethodDto>().ReverseMap();
    }
}
