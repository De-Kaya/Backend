using Data.Entities;
using Data.Models;
using Domain.Dtos;

namespace Data.Interfaces;

public interface IRoomRepository : IBaseRepository<RoomEntity, RoomDto>
{
    Task<ApiResponse<IEnumerable<RoomDto>>> GetAvailableRoomAsync();
    Task<ApiResponse<IEnumerable<RoomDto>>> GetRoomByStatusAsync(string statusName);
}
