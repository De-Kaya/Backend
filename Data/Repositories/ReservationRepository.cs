using AutoMapper;
using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories;

public class ReservationRepository(DataContext context, IMapper mapper) : BaseRepository<ReservationEntity, ReservationDto>(context, mapper), IReservationRepository
{
    //Oda için belirtilen tarihler arasında çakışan rezervasyon olup olmadığını kontrol eder
    public async Task<ApiResponse<bool>> CheckReservationConflictAsync(int roomId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var hasConflict = await _table.AnyAsync(r =>
                r.RoomId == roomId &&
                r.IsActive &&
                startDate < r.EndDate && endDate > r.StartDate
            );
            return new ApiResponse<bool> { Succeeded = true, StatusCode = 200, Message = "Conflict check completed", Result = hasConflict };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = false };
        }
    }

    //Aktif rezervasyonları getirir
    public async Task<ApiResponse<IEnumerable<ReservationDto>>> GetActiveReservationsAsync()
    {
        return await GetAllAsync(
            where: r => r.IsActive,
            includes: [ r => r.Room, r => r.Customer ]
            );
    }

    //Belirtilen tarih aralığındaki rezervasyonları getirir
    public async Task<ApiResponse<IEnumerable<ReservationDto>>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await GetAllAsync(
            where: r => r.StartDate <= endDate && r.EndDate >= startDate && r.IsActive,
            includes: [r => r.Room, r => r.Customer]
        );
    }

    //Süresi dolmuş rezervasyonları getirir
    public async Task<ApiResponse<IEnumerable<ReservationDto>>> GetExpiredReservationsAsync()
    {
        return await GetAllAsync(
            where: r => r.EndDate < DateTime.UtcNow && r.IsActive,
            includes: new Expression<Func<ReservationEntity, object>>[] { r => r.Room, r => r.Customer }
        );
    }
}
