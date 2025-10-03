using Data.Entities;
using Data.Models;
using Domain.Dtos;

namespace Data.Interfaces;

public interface IRoomStatusRepository : IBaseRepository<RoomStatusEntity, RoomStatusDto>
{
    Task<ApiResponse<RoomStatusDto>> GetStatusByNameAsync(string statusName);
}