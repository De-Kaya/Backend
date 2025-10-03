using Data.Entities;
using Data.Models;
using Domain.Dtos.Payment;

namespace Data.Interfaces;

public interface IPaymentRepository : IBaseRepository<PaymentEntity, PaymentDto>
{
    Task<ApiResponse<decimal>> GetTotalPaymnetsByCustomerAsync(string customerId);
    Task<ApiResponse<IEnumerable<PaymentDto>>> GetPendingPaymentsAsync();
}
