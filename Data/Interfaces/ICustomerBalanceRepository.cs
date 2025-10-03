using Data.Entities;
using Data.Models;
using Domain.Dtos;

namespace Data.Interfaces;

public interface ICustomerBalanceRepository : IBaseRepository<CustomerBalanceEntity, CustomerBalanceDto>
{
    Task<ApiResponse<decimal>> GetTotalBalanceAsync(string customerId);
    Task<ApiResponse<IEnumerable<CustomerBalanceDto>>> GetBalanceDetailsAsync(string customerId);
    Task<ApiResponse<IEnumerable<CustomerBalanceDto>>> GetRecentTransactionsAsync(string customerId, int count);
}
