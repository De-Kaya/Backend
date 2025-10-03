using AutoMapper;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos;

namespace Business.Services;

public class ReservationService(IReservationRepository reservationRepository, ICustomerRepository customerRepository, IRoomRepository roomRepository, ICustomerBalanceRepository customerBalanceRepository,IMapper mapper) : IReservationService
{
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly IRoomRepository _roomRepository = roomRepository;
    private readonly ICustomerBalanceRepository _customerBalanceRepository = customerBalanceRepository;
    private readonly IMapper _mapper = mapper;


    public async Task<ApiResponse<IEnumerable<ReservationDto>>> GetAllReservationAsync()
    {
        try
        {
            return await _reservationRepository.GetAllAsync(orderByDescending: false, sortBy: null, where: null, r => r.Room, r => r.Customer);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<ReservationDto>> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    public async Task<ApiResponse<ReservationDto>> GetReservationByIdAsync(string id)
    {
        try
        {
            if(string.IsNullOrEmpty(id))
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 400, Message = "Invalid reservation ID", Result = null };

            var result = await _reservationRepository.GetAsync(r => r.Id == id, r => r.Room, r => r.Customer);
            if (!result.Succeeded || result.Result == null)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 404, Message = "Reservation not found", Result = null };

            return new ApiResponse<ReservationDto> { Succeeded = true, StatusCode = 200, Message = "Reservation retrieved successfully", Result = result.Result };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    //Rezervasyon oluştur
    public async Task<ApiResponse<ReservationDto>> CreateReservationAsync(ReservationDto reservationDto)
    {
        try
        {
            // Tarih doğrulama
            if (reservationDto.StartDate >= reservationDto.EndDate)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 400, Message = "Start date must be before end date", Result = null };

            // Müşteri kontrolü
            var customerResult = await _customerRepository.GetAsync(c => c.Id == reservationDto.CustomerId);
            if (!customerResult.Succeeded || customerResult.Result == null)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 404, Message = "Customer not found", Result = null };

            // Oda kontrolü
            var roomResult = await _roomRepository.GetAsync(r => r.Id == reservationDto.RoomId);
            if (!roomResult.Succeeded || roomResult.Result == null)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 404, Message = "Room not found", Result = null };

            // Çakışma kontrolü
            var conflictCheck = await _reservationRepository.CheckReservationConflictAsync(reservationDto.RoomId, reservationDto.StartDate, reservationDto.EndDate);
            if (!conflictCheck.Succeeded)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 500, Message = conflictCheck.Message ?? "Failed to check reservation conflicts", Result = null };
            if (conflictCheck.Result)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 400, Message = "The room is already booked for the selected dates", Result = null };

            await _reservationRepository.BeginTransactionAsync();
            try
            {
                // Rezervasyon oluştur
                var reservationEntity = _mapper.Map<ReservationEntity>(reservationDto);
                reservationEntity.IsActive = true;
                reservationEntity.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"));
                var addResult = await _reservationRepository.AddAsync(reservationEntity);
                if (!addResult.Succeeded)
                {
                    await _reservationRepository.RollbackTransactionAsync();
                    return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 500, Message = "Failed to create reservation", Result = null };
                }

                // Müşteri bakiyesi güncelle
                var balanceDto = new CustomerBalanceDto
                {
                    CustomerId = reservationDto.CustomerId,
                    Amount = reservationDto.Price,
                    TransactionType = TransactionType.Debt,
                    ReservationId = reservationEntity.Id,
                    TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul")),
                    Description = $"Rezervasyon #{reservationEntity.Id} için borç"
                };
                var balanceEntity = _mapper.Map<CustomerBalanceEntity>(balanceDto);
                var balanceResult = await _customerBalanceRepository.AddAsync(balanceEntity);
                if (!balanceResult.Succeeded)
                {
                    await _reservationRepository.RollbackTransactionAsync();
                    return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = balanceResult.StatusCode, Message = "Müşteri bakiyesi güncellenemedi: " + balanceResult.Message, Result = null };
                }

                await _reservationRepository.CommitTransactionAsync();
                var resultDto = _mapper.Map<ReservationDto>(reservationEntity);
                return new ApiResponse<ReservationDto> { Succeeded = true, StatusCode = 201, Message = "Reservation created successfully", Result = resultDto };
            }
            catch
            {
                await _reservationRepository.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    public async Task<ApiResponse<ReservationDto>> UpdateReservationAsync(ReservationDto reservationDto)
    {
        try
        {
            // Tarih doğrulama
            if (reservationDto.StartDate >= reservationDto.EndDate)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 400, Message = "Start date must be before end date", Result = null };

            // Rezervasyon kontrolü
            var existingReservation = await _reservationRepository.GetAsync(r => r.Id == reservationDto.Id, r => r.Room, r => r.Customer);
            if (!existingReservation.Succeeded || existingReservation.Result == null)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 404, Message = "Reservation not found", Result = null };

            // Müşteri kontrolü
            var customerResult = await _customerRepository.GetAsync(c => c.Id == reservationDto.CustomerId);
            if (!customerResult.Succeeded || customerResult.Result == null)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 404, Message = "Customer not found", Result = null };

            // Oda kontrolü
            var roomResult = await _roomRepository.GetAsync(r => r.Id == reservationDto.RoomId);
            if (!roomResult.Succeeded || roomResult.Result == null)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 404, Message = "Room not found", Result = null };

            // Çakışma kontrolü (mevcut rezervasyon hariç)
            var conflictCheck = await _reservationRepository.CheckReservationConflictAsync(reservationDto.RoomId, reservationDto.StartDate, reservationDto.EndDate);
            if (!conflictCheck.Succeeded)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 500, Message = conflictCheck.Message ?? "Failed to check reservation conflicts", Result = null };
            if (conflictCheck.Result && reservationDto.RoomId != existingReservation.Result.RoomId)
                return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 400, Message = "The room is already booked for the selected dates", Result = null };

            await _reservationRepository.BeginTransactionAsync();
            try
            {
                // Rezervasyonu güncelle
                var reservationEntity = _mapper.Map<ReservationEntity>(reservationDto);
                reservationEntity.CreatedAt = existingReservation.Result.CreatedAt; // CreatedAt'i koru
                var updateResult = await _reservationRepository.UpdateAsync(reservationEntity);
                if (!updateResult.Succeeded)
                {
                    await _reservationRepository.RollbackTransactionAsync();
                    return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 500, Message = "Failed to update reservation", Result = null };
                }

                // Fiyat farkı varsa müşteri bakiyesi güncelle
                if (existingReservation.Result.Price != reservationDto.Price)
                {
                    var balanceDto = new CustomerBalanceDto
                    {
                        CustomerId = reservationDto.CustomerId,
                        Amount = reservationDto.Price - existingReservation.Result.Price,
                        TransactionType = reservationDto.Price > existingReservation.Result.Price ? TransactionType.Debt : TransactionType.Refund,
                        ReservationId = reservationEntity.Id,
                        TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul")),
                        Description = $"Rezervasyon #{reservationEntity.Id} güncellemesi sonrası fiyat farkı"
                    };
                    var balanceEntity = _mapper.Map<CustomerBalanceEntity>(balanceDto);
                    var balanceResult = await _customerBalanceRepository.AddAsync(balanceEntity);
                    if (!balanceResult.Succeeded)
                    {
                        await _reservationRepository.RollbackTransactionAsync();
                        return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = balanceResult.StatusCode, Message = "Müşteri bakiyesi güncellenemedi: " + balanceResult.Message, Result = null };
                    }
                }

                await _reservationRepository.CommitTransactionAsync();
                var resultDto = _mapper.Map<ReservationDto>(reservationEntity);
                return new ApiResponse<ReservationDto> { Succeeded = true, StatusCode = 200, Message = "Reservation updated successfully", Result = resultDto };
            }
            catch
            {
                await _reservationRepository.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return new ApiResponse<ReservationDto> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    public async Task<ApiResponse<bool>> DeleteReservationAsync(string reservationId)
    {
        try
        {
            var reservationResult = await _reservationRepository.GetAsync(r => r.Id == reservationId);
            if (!reservationResult.Succeeded || reservationResult.Result == null)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 404, Message = "Reservation not found", Result = false };

            await _reservationRepository.BeginTransactionAsync();
            try
            {
                // Rezervasyonu sil
                var reservationEntity = _mapper.Map<ReservationEntity>(reservationResult.Result);
                var deleteResult = await _reservationRepository.DeleteAsync(reservationEntity);
                if (!deleteResult.Succeeded)
                {
                    await _reservationRepository.RollbackTransactionAsync();
                    return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = "Failed to delete reservation", Result = false };
                }

                // Müşteri bakiyesine iade ekle
                var balanceDto = new CustomerBalanceDto
                {
                    CustomerId = reservationResult.Result.CustomerId,
                    Amount = reservationResult.Result.Price,
                    TransactionType = TransactionType.Refund,
                    ReservationId = reservationEntity.Id,
                    TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul")),
                    Description = $"Rezervasyon #{reservationEntity.Id} iptali sonrası iade"
                };
                var balanceEntity = _mapper.Map<CustomerBalanceEntity>(balanceDto);
                var balanceResult = await _customerBalanceRepository.AddAsync(balanceEntity);
                if (!balanceResult.Succeeded)
                {
                    await _reservationRepository.RollbackTransactionAsync();
                    return new ApiResponse<bool> { Succeeded = false, StatusCode = balanceResult.StatusCode, Message = "Müşteri bakiyesi güncellenemedi: " + balanceResult.Message, Result = false };
                }

                await _reservationRepository.CommitTransactionAsync();
                return new ApiResponse<bool> { Succeeded = true, StatusCode = 200, Message = "Reservation deleted successfully", Result = true };
            }
            catch
            {
                await _reservationRepository.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = false };
        }
    }

    //Aktif rezervasyonları getir
    public async Task<ApiResponse<IEnumerable<ReservationDto>>> GetActiveReservationsAsync()
    {
        try
        {
            var result = await _reservationRepository.GetActiveReservationsAsync();
            return result;
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<ReservationDto>> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    //Süresi dolmuş rezervasyonları getir
    public async Task<ApiResponse<IEnumerable<ReservationDto>>> GetExpiredReservationsAsync()
    {
        try
        {
            var result = await _reservationRepository.GetExpiredReservationsAsync();
            return result;
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<ReservationDto>> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    //Belirtilen tarih aralığındaki rezervasyonları getir
    public async Task<ApiResponse<IEnumerable<ReservationDto>>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
                return new ApiResponse<IEnumerable<ReservationDto>> { Succeeded = false, StatusCode = 400, Message = "Start date must be before end date", Result = null };

            var result = await _reservationRepository.GetReservationsByDateRangeAsync(startDate, endDate);
            return result;
        }
        catch (Exception ex) 
        {
            return new ApiResponse<IEnumerable<ReservationDto>> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }
}
