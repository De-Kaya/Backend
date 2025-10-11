using Data.Entities;
using Data.Models;
using Domain.Dtos;

namespace Data.Interfaces;

public interface ICustomerRepository : IBaseRepository<CustomerEntity, CustomerDto>
{
    Task<ApiResponse<int>> GetCustomerCountAsync();
    Task<ApiResponse<int>> GetReservationCountAsync(string customerId);
    Task<ApiResponse<IEnumerable<CustomerDto>>> GetCustomersWithOverdueBalanceAsync();
    Task<ApiResponse<PageResult<CustomerDto>>> GetPagedAsync(int page, int pageSize, string? search);
}
