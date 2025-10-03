using AutoMapper;
using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories;

public class CustomerBalanceRepository(DataContext context, IMapper mapper) : BaseRepository<CustomerBalanceEntity, CustomerBalanceDto>(context, mapper), ICustomerBalanceRepository
{
    //Müşterinin tüm bakiye işlemlerini detaylarıyla getirir(örn: borç, ödeme, iade).
    public async Task<ApiResponse<IEnumerable<CustomerBalanceDto>>> GetBalanceDetailsAsync(string customerId)
    {
        return await GetAllAsync(
            where: b => b.CustomerId == customerId,
            includes: [ b => b.Customer, b => b.Reservation!, b => b.Payment!]
        );
    }

   

    // Müşterinin toplam bakiyesini getirir.
    public async Task<ApiResponse<decimal>> GetTotalBalanceAsync(string customerId)
    {
        try
        {
            var balance = await _table
                .Where(b => b.CustomerId == customerId)
                .SumAsync(b => b.TransactionType == TransactionType.Debt || b.TransactionType == TransactionType.Refund 
                    ? b.Amount 
                    : -b.Amount);

            return new ApiResponse<decimal> { Succeeded = true, StatusCode = 200, Message = balance != 0 ? "Total balance retrieved successfully" : "No balance transaction", Result = balance };
        }
        catch (Exception ex)
        {
            return new ApiResponse<decimal> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = 0 };
        }
    }

    //Müşterinin son işlemlerini getirir
    public async Task<ApiResponse<IEnumerable<CustomerBalanceDto>>> GetRecentTransactionsAsync(string customerId, int count)
    {
        var result = await GetAllAsync(
            where: b => b.CustomerId == customerId,
            sortBy: b => b.TransactionDate,
            orderByDescending: true,
            includes: [b => b.Customer, b => b.Reservation!, b => b.Payment!]
        );

        if (result.Succeeded && result.Result != null)
            result.Result = result.Result.Take(count);
        return result;
    }
}
