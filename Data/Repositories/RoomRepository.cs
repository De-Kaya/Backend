using AutoMapper;
using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos;

namespace Data.Repositories;

public class RoomRepository(DataContext context, IMapper mapper) : BaseRepository<RoomEntity, RoomDto>(context, mapper), IRoomRepository
{

    //Sadece "Müsait" durumundaki odaları getirir
    public async Task<ApiResponse<IEnumerable<RoomDto>>> GetAvailableRoomAsync()
    {
        return await GetAllAsync(
            where: r => r.Status.StatusName == "Müsait",
            includes: r => r.Status
            );
    }

    //Belirtilen duruma sahip odaları getirir
    public async Task<ApiResponse<IEnumerable<RoomDto>>> GetRoomByStatusAsync(string statusName)
    {
        return await GetAllAsync
            (
            where: r => r.Status.StatusName == statusName,
            includes: r => r.Status
            );
    }
}
