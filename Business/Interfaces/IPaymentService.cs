using Data.Models;
using Domain.Dtos.Payment;

namespace Business.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<CreatePaymentResponseDto>> CreatePaymentAsync(CreatePaymentRequestDto paymentRequestDto);
    Task<ApiResponse<PaymentDto>> UpdatePaymentAsync(PaymentDto paymentDto);
    Task<ApiResponse<bool>> DeletePaymentAsync(string paymentId);
    Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(string paymentId);
    Task<ApiResponse<IEnumerable<PaymentDto>>> GetAllPaymentsAsync();
    Task<ApiResponse<decimal>> GetTotalPaymentsByCustomerAsync(string customerId);
    Task<ApiResponse<IEnumerable<PaymentDto>>> GetPendingPaymentsAsync();
}
