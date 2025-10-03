using Data.Entities;
using Data.Models;
using Domain.Dtos;

namespace Data.Interfaces;

public interface IMaintenanceLogRepository : IBaseRepository<MaintenanceLogEntity, MaintenancelLogDto>
{
    Task<ApiResponse<IEnumerable<MaintenancelLogDto>>> GetUnresolvedLogsAsync();
}
