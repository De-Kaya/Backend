using Data.Models;
using Domain.Dtos;

namespace Business.Interfaces;

public interface IReservationService
{
    Task<ApiResponse<IEnumerable<ReservationDto>>> GetAllReservationAsync();
    Task<ApiResponse<ReservationDto>> GetReservationByIdAsync(string id);
    Task<ApiResponse<ReservationDto>> CreateReservationAsync(ReservationDto reservationDto);
    Task<ApiResponse<ReservationDto>> UpdateReservationAsync(ReservationDto reservationDto);
    Task<ApiResponse<bool>> DeleteReservationAsync(string reservationId);
    Task<ApiResponse<IEnumerable<ReservationDto>>> GetActiveReservationsAsync();
    Task<ApiResponse<IEnumerable<ReservationDto>>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<ApiResponse<IEnumerable<ReservationDto>>> GetExpiredReservationsAsync();
}
