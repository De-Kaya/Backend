using AutoMapper;
using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos.Payment;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories;

public class PaymentRepository(DataContext context, IMapper mapper) : BaseRepository<PaymentEntity, PaymentDto>(context, mapper), IPaymentRepository
{
    // Müşteri bazında yapılan toplam ödemeleri getirir
    public async Task<ApiResponse<decimal>> GetTotalPaymnetsByCustomerAsync(string customerId)
    {
        try
        {
            var total = await _table
                .Where(p => p.Reservation.CustomerId == customerId && p.IsPaid)
                .SumAsync(p => p.Amount);
            return new ApiResponse<decimal> { Succeeded = true, StatusCode = 200, Message = "Total payments retrieved successfully", Result = total };
        }
        catch (Exception ex)
        {
           return new ApiResponse<decimal> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = 0 };
        }
    }

    // Ödenmemiş tüm ödemeleri detaylarıyla birlikte getirir
    public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetPendingPaymentsAsync()
    {
        return await GetAllAsync(
            where: p => !p.IsPaid,
            includes: [p => p.Reservation, p => p.PaymentMethod]
        );
    }
}
