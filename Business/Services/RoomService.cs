using AutoMapper;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos;

namespace Business.Services;

public class RoomService(IRoomRepository roomRepository, IRoomStatusRepository roomStatusRepository, IMapper mapper) : IRoomService
{
    private readonly IRoomRepository _roomRepository = roomRepository;
    private readonly IRoomStatusRepository _roomStatusRepository = roomStatusRepository;
    private readonly IMapper _mapper = mapper;


    //Tüm odaları getir
    public async Task<ApiResponse<IEnumerable<RoomDto>>> GetAllRoomsAsync()
    {
        return await _roomRepository.GetAllAsync();
    }

    //Oda olustur
    public async Task<ApiResponse<RoomDto>> CreateRoomAsync(RoomDto roomDto)
    {
        try
        {
            //var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"));
            //Serial number'ın benzersiz olup olmadığını kontrol et
            var existingRoom = await _roomRepository.GetAsync(r => r.SerialNumber ==  roomDto.SerialNumber);
            if (existingRoom.Succeeded && existingRoom.Result != null)
                return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 400, Message = "Room with this serial number already exists", Result = null };

            //Room statusunun geçerli olup olmadığını kontrol et
            var roomStatus = await _roomStatusRepository.GetStatusByNameAsync(roomDto.StatusName);
            if (!roomStatus.Succeeded || roomStatus.Result == null)
                return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 400, Message = roomStatus.Message ?? "Invalid room status", Result = null };
            
            //Odayı oluştur
            var roomEntity = _mapper.Map<RoomEntity>(roomDto);
            roomEntity.StatusId = roomStatus.Result.Id;
            var createdRoom = await _roomRepository.AddAsync(roomEntity);
            if (!createdRoom.Succeeded)
                return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 500, Message = "Failed to create room", Result = null };

            //Oluşturulan odayı DTO'ya dönüştür ve döndür
            var createdRoomDto = _mapper.Map<RoomDto>(roomEntity);
            return new ApiResponse<RoomDto> { Succeeded = true, StatusCode = 201, Message = "Room created successfully", Result = createdRoomDto };
        }
        catch (Exception ex)
        {
            return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    //Oda guncelle
    public async Task<ApiResponse<RoomDto>> UpdateRoomAsync(RoomDto roomDto)
    {
        try
        {
            //odayı kontrol et
            var existingRoom = await _roomRepository.GetAsync(r => r.Id == roomDto.Id, r => r.Status);
            if (!existingRoom.Succeeded || existingRoom.Result == null)
                return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 404, Message = "Room not found", Result = null };

            //Serial number'ın benzersiz olup olmadığını kontrol et
            var serialNumberCheck = await _roomRepository.GetAsync(r => r.SerialNumber == roomDto.SerialNumber && r.Id != roomDto.Id);
            if (serialNumberCheck.Succeeded && serialNumberCheck.Result != null)
                return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 400, Message = "Another room with this serial number already exists", Result = null };

            //Room statusunun geçerli olup olmadığını kontrol et
            var statusCheck = await _roomStatusRepository.GetStatusByNameAsync(roomDto.StatusName);
            if (!statusCheck.Succeeded || statusCheck.Result == null)
                return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 400, Message = statusCheck.Message ?? "Invalid room status", Result = null };

            //Odayı güncelle
            var roomEntity = _mapper.Map<RoomEntity>(roomDto);
            roomEntity.StatusId = statusCheck.Result.Id;
            var uppdateResult = await _roomRepository.UpdateAsync(roomEntity);
            if (!uppdateResult.Succeeded)
                return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 500, Message = "Failed to update room", Result = null };

            //Güncellenen odayı DTO'ya dönüştür ve döndür
            var updatedRoomDto = _mapper.Map<RoomDto>(roomEntity);
                return new ApiResponse<RoomDto> { Succeeded = true, StatusCode = 200, Message = "Room updated successfully", Result = updatedRoomDto };

        }
        catch (Exception ex)
        {
            return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    //Oda sil
    public async Task<ApiResponse<bool>> DeleteRoomAsync(int roomId)
    {
        try
        {
            var existingRoom = await _roomRepository.GetAsync(r => r.Id == roomId, r => r.Reservations);
            if (!existingRoom.Succeeded || existingRoom.Result == null)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 404, Message = "Room not found", Result = false };

            //Aktif rezervasyon kontrolü
            var hasActiveReservations = await _roomRepository.ExistsAsync(r => r.Id == roomId && r.Reservations.Any(res => res.IsActive));
            if (hasActiveReservations.Succeeded && hasActiveReservations.Result)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 400, Message = "Cannot delete room with active reservations", Result = false };

            var roomEntity = _mapper.Map<RoomEntity>(existingRoom.Result);
            var deleteResult = await _roomRepository.DeleteAsync(roomEntity);
            if (!deleteResult.Succeeded)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = "Failed to delete room", Result = false };

            return new ApiResponse<bool> { Succeeded = true, StatusCode = 200, Message = "Room deleted successfully", Result = true };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = false };
        }
    }

    //Müsait odaları getir
    public async Task<ApiResponse<IEnumerable<RoomDto>>> GetAvailableRoomsAsync()
    {
        return await _roomRepository.GetAvailableRoomAsync();
    }

    //Oda statusuna göre odaları getir
    public async Task<ApiResponse<IEnumerable<RoomDto>>> GetRoomsByStatusAsync(string statusName)
    {
        if (string.IsNullOrWhiteSpace(statusName))
            return new ApiResponse<IEnumerable<RoomDto>> { Succeeded = false, StatusCode = 400, Message = "Status name cannot be empty", Result = null };

        return await _roomRepository.GetRoomByStatusAsync(statusName);
    }


    //Oda durumunu güncelle
    public async Task<ApiResponse<RoomDto>> UpdateRoomStatusAsync(int roomId, string statusName)
    {
        try
        {
            var statusResult = await _roomStatusRepository.GetStatusByNameAsync(statusName);
            if (!statusResult.Succeeded || statusResult.Result == null)
                return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 400, Message = statusResult.Message ?? "Invalid room status", Result = null };

            var roomResult = await _roomRepository.GetAsync(r => r.Id == roomId, r => r.Status);
            if (!roomResult.Succeeded || roomResult.Result == null)
                return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 404, Message = "Room not found", Result = null };

            var roomEntity = _mapper.Map<RoomEntity>(roomResult.Result);
            roomEntity.StatusId = statusResult.Result.Id;
            var updateResult = await _roomRepository.UpdateAsync(roomEntity);
            if (!updateResult.Succeeded)
                return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 500, Message = "Failed to update room status", Result = null };

            var updatedRoomDto = _mapper.Map<RoomDto>(roomEntity);
            return new ApiResponse<RoomDto> { Succeeded = true, StatusCode = 200, Message = "Room status updated successfully", Result = updatedRoomDto };
        }
        catch (Exception ex)
        {
            return new ApiResponse<RoomDto> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }
}
