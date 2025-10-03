using Data.Models;
using Domain.Dtos;

namespace Business.Interfaces;

public interface IRoomService
{
    Task<ApiResponse<IEnumerable<RoomDto>>> GetAllRoomsAsync();
    Task<ApiResponse<RoomDto>> CreateRoomAsync(RoomDto roomDto);
    Task<ApiResponse<RoomDto>> UpdateRoomAsync(RoomDto roomDto);
    Task<ApiResponse<bool>> DeleteRoomAsync(int roomId);
    Task<ApiResponse<RoomDto>> UpdateRoomStatusAsync(int roomId, string statusName);
    Task<ApiResponse<IEnumerable<RoomDto>>> GetAvailableRoomsAsync();
    Task<ApiResponse<IEnumerable<RoomDto>>> GetRoomsByStatusAsync(string statusName);
}
