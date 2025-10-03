using AutoMapper;
using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public class CustomerRepository(DataContext context, IMapper mapper) : BaseRepository<CustomerEntity, CustomerDto>(context, mapper), ICustomerRepository
{
    //Müşteri sayısını getirir
    public Task<ApiResponse<int>> GetCustomerCountAsync()
    {
        try
        {
            var count = _table.CountAsync();
            return count.ContinueWith(t => new ApiResponse<int>
            {
                Succeeded = true,
                StatusCode = 200,
                Message = t.Result > 0 ? "Customer count retrieved successfully" : "No customers found",
                Result = t.Result
            });
        }
        catch (Exception ex)
        {
          return Task.FromResult(new ApiResponse<int>
          {
              Succeeded = false,
              StatusCode = 500,
              Message = ex.Message,
              Result = 0
          });
        }
    }

    //Belirtilen müşterinin rezervasyon sayısını getirir
    public async Task<ApiResponse<int>> GetReservationCountAsync(string customerId)
    {
        try
        {
            var count = await _table
                .Where(c => c.Id == customerId)
                .Select(c => c.Reservations.Count)
                .FirstOrDefaultAsync();
            return new ApiResponse<int>
            { 
                Succeeded = true,
                StatusCode = 200,
                Message = count > 0 ? "Reservation count retrieved successfully" : "No reservations found",
                Result = count
            };
        }
        catch (Exception ex)
        {
          return new ApiResponse<int>
          {
              Succeeded = false,
              StatusCode = 500,
              Message = ex.Message,
              Result = 0
          };
        }
    }

    public async Task<ApiResponse<IEnumerable<CustomerDto>>> GetCustomersWithOverdueBalanceAsync()
    {
        try
        {
            var customers = await _table
                .Include(c => c.CustomerBalances)
                .Select(c => new
                {
                    Customer = c,
                    Balance = c.CustomerBalances.Sum(b => b.TransactionType == TransactionType.Debt || b.TransactionType == TransactionType.Refund
                        ? b.Amount
                        : -b.Amount)
                })
                .Where(c => c.Balance > 0)
                .Select(c => c.Customer)
                .ToListAsync();

            var result = _mapper.Map<IEnumerable<CustomerDto>>(customers);
            return new ApiResponse<IEnumerable<CustomerDto>>
            {
                Succeeded = true,
                StatusCode = 200,
                Message = result.Any() ? "Customers with overdue balance retrieved successfully" : "No customers with overdue balance",
                Result = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<CustomerDto>>
            {
                Succeeded = false,
                StatusCode = 500,
                Message = ex.Message,
                Result = null
            };
        }
    }
}
