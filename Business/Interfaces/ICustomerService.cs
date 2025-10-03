using Data.Models;
using Domain.Dtos;

namespace Business.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<IEnumerable<CustomerDto>>> GetAllCustomersAsync();
    Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(string customerId);
    Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerDto customerDto);
    Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(CustomerDto customerDto);
    Task<ApiResponse<bool>> DeleteCustomerAsync(string customerId);
    Task<ApiResponse<decimal>> GetCustomerBalanceAsync(string customerId);
    Task<ApiResponse<IEnumerable<ReservationDto>>> GetCustomerReservationsAsync(string customerId);
    Task<ApiResponse<int>> GetCustomerCountAsync();
    Task<ApiResponse<int>> GetReservationCountAsync(string customerId);
    Task<ApiResponse<IEnumerable<CustomerDto>>> GetCustomersWithOverdueBalanceAsync();

}
