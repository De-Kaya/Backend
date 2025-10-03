using Data.Entities;
using Data.Models;
using Domain.Dtos;

namespace Data.Interfaces;

public interface IPaymentMethodRepository : IBaseRepository<PaymentMethodEntity, PaymentMethodDto>
{
    Task<ApiResponse<IEnumerable<PaymentMethodDto>>> GetAllMethodsAsync();
}
