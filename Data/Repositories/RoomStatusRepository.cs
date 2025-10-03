using AutoMapper;
using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public class RoomStatusRepository(DataContext context, IMapper mapper) : BaseRepository<RoomStatusEntity, RoomStatusDto>(context, mapper), IRoomStatusRepository
{

    public async Task<ApiResponse<RoomStatusDto>> GetStatusByNameAsync(string statusName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(statusName))
                return new ApiResponse<RoomStatusDto> { Succeeded = false, StatusCode = 400, Message = "Status name cannot be null or empty", Result = null };

            var status = await _table
                .Where(s => s.StatusName == statusName)
                .FirstOrDefaultAsync();

            if (status == null)
                return new ApiResponse<RoomStatusDto> { Succeeded = false, StatusCode = 404, Message = $"Status '{statusName}' not found", Result = null };

            var statusDto = _mapper.Map<RoomStatusDto>(status);
            return new ApiResponse<RoomStatusDto> { Succeeded = true, StatusCode = 200, Message = "Status retrieved successfully", Result = statusDto };
        }
        catch (Exception ex)
        { 
            return new ApiResponse<RoomStatusDto> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }
}