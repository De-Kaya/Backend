using AutoMapper;
using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos;

namespace Data.Repositories;

public class MaintenanceLogRepository(DataContext context, IMapper mapper) : BaseRepository<MaintenanceLogEntity, MaintenancelLogDto>(context, mapper), IMaintenanceLogRepository
{
    //Çözülmemiş bakım kayıtlarını getirir.
    public async Task<ApiResponse<IEnumerable<MaintenancelLogDto>>> GetUnresolvedLogsAsync()
    {
        return await GetAllAsync(
            where: m => !m.IsResolved,
            includes: m => m.Room
        );
    }
}
