using AutoMapper;
using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos;

namespace Data.Repositories;

public class PaymentMethodRepository(DataContext context, IMapper mapper) : BaseRepository<PaymentMethodEntity, PaymentMethodDto>(context, mapper), IPaymentMethodRepository
{
    public async Task<ApiResponse<IEnumerable<PaymentMethodDto>>> GetAllMethodsAsync()
    {
        return await GetAllAsync();
    }
}
