using Data.Entities;
using Data.Models;
using Domain.Dtos;

namespace Data.Interfaces;

public interface IReservationRepository : IBaseRepository<ReservationEntity, ReservationDto>
{
    Task<ApiResponse<IEnumerable<ReservationDto>>> GetActiveReservationsAsync();
    Task<ApiResponse<bool>> CheckReservationConflictAsync(int roomId, DateTime startDate, DateTime endDate);
    Task<ApiResponse<IEnumerable<ReservationDto>>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<ApiResponse<IEnumerable<ReservationDto>>> GetExpiredReservationsAsync();
}
