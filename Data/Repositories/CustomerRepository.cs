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
    public async Task<ApiResponse<int>> GetCustomerCountAsync()
    {
        try
        {
            var count = await _table.CountAsync();
            return new ApiResponse<int>
            {
                Succeeded = true,
                StatusCode = 200,
                Message = count > 0 ? "Customer count retrieved successfully" : "No customers found",
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
                    Balance = c.CustomerBalances.Sum(b => b.TransactionType == TransactionType.Debt
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

    public async Task<ApiResponse<PageResult<CustomerDto>>> GetPagedAsync(int page, int pageSize, string? search)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            IQueryable<CustomerEntity> query = _table.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(c =>
                    (c.FullName != null && c.FullName.ToLower().Contains(s)) ||
                    (c.Email != null && c.Email.ToLower().Contains(s)));
            }

            query = query.OrderByDescending(c => c.CreatedAt);
            var total = await query.CountAsync();

            var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            var items = _mapper.Map<IEnumerable<CustomerDto>>(entities);

            var result = new PageResult<CustomerDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };

            return new ApiResponse<PageResult<CustomerDto>>
            {
                Succeeded = true,
                StatusCode = 200,
                Message = "Customers paged successfully",
                Result = result
            };

        }
        catch (Exception ex)
        {
            return new ApiResponse<PageResult<CustomerDto>>
            {
                Succeeded = false,
                StatusCode = 500,
                Message = ex.Message,
                Result = null
            };
        }
    }
}
