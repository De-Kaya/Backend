using AutoMapper;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos.Payment;
using Microsoft.Extensions.Logging;

namespace Business.Services;

public class PaymentService(IPaymentRepository paymentRepository, IReservationRepository reservationRepository, IMapper mapper) : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly IMapper _mapper = mapper;


    public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetAllPaymentsAsync()
    {
        try
        {
            var result = await _paymentRepository.GetAllAsync(includes: [p => p.Reservation!, p => p.PaymentMethod!]);

            if (!result.Succeeded)
            {
                return new ApiResponse<IEnumerable<PaymentDto>> { Succeeded = false, StatusCode = 500, Message = "Failed to retrieve payments", Result = null };
            }

            return new ApiResponse<IEnumerable<PaymentDto>> { Succeeded = true, StatusCode = 200, Message = "Payments retrieved successfully", Result = result.Result };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<PaymentDto>> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    public async Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(string paymentId)
    {
        try
        {
            if (string.IsNullOrEmpty(paymentId))
                return new ApiResponse<PaymentDto> { Succeeded = false, StatusCode = 400, Message = "Payment ID is required", Result = null };

            var paymentResult = await _paymentRepository.GetAsync(p => p.Id == paymentId, includes: [p => p.Reservation, p => p.PaymentMethod]);
            if (!paymentResult.Succeeded || paymentResult.Result == null)
                return new ApiResponse<PaymentDto> { Succeeded = false, StatusCode = 404, Message = "Payment not found", Result = null };

            return new ApiResponse<PaymentDto> { Succeeded = true, StatusCode = 200, Message = "Payment retrieved successfully", Result = paymentResult.Result };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PaymentDto> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }
    
    public async Task<ApiResponse<CreatePaymentResponseDto>> CreatePaymentAsync(CreatePaymentRequestDto paymentRequestDto)
    {
        try
        {
            //Rezervasyon ID'si
            if (string.IsNullOrEmpty(paymentRequestDto.ReservationId))
                return new ApiResponse<CreatePaymentResponseDto> { Succeeded = false, StatusCode = 400, Message = "Reservation Id is required", Result = null };

            if (paymentRequestDto.Amount <= 0)
                return new ApiResponse<CreatePaymentResponseDto> { Succeeded = false, StatusCode = 400, Message = "Amount must be greater than zero", Result = null };

            //Rezervasyon Kontrolü
            var reservationCheck = await _reservationRepository.GetAsync(r => r.Id == paymentRequestDto.ReservationId);
            if (!reservationCheck.Succeeded || reservationCheck.Result == null)
                return new ApiResponse<CreatePaymentResponseDto> { Succeeded = false, StatusCode = 404, Message = "Reservation not found", Result = null };

            //Entity Dönüşümü
            var paymentEntity = new PaymentEntity
            {
                Id = Guid.NewGuid().ToString(),
                ReservationId = paymentRequestDto.ReservationId,
                Amount = paymentRequestDto.Amount,
                PaymentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul")),
                PaymentMethodId = paymentRequestDto.PaymentMethodId,
                IsPaid = paymentRequestDto.IsPaid
            };

            var addResult = await _paymentRepository.AddAsync(paymentEntity);
            if (!addResult.Succeeded)
                return new ApiResponse<CreatePaymentResponseDto> { Succeeded = false, StatusCode = 500, Message = "Payment could not be created", Result = null };

            //Response Dto Dönüşümü
            var resultDto = new CreatePaymentResponseDto
            {
                Id = paymentEntity.Id,
                ReservationId = paymentEntity.ReservationId,
                Amount = paymentEntity.Amount,
                PaymentDate = paymentEntity.PaymentDate,
                PaymentMethodId = paymentEntity.PaymentMethodId,
                IsPaid = paymentEntity.IsPaid
            };

            return new ApiResponse<CreatePaymentResponseDto> { Succeeded = true, StatusCode = 201, Message = "Payment created successfully", Result = resultDto };

        }
        catch (Exception ex)
        {
            return new ApiResponse<CreatePaymentResponseDto> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    public async Task<ApiResponse<PaymentDto>> UpdatePaymentAsync(PaymentDto paymentDto)
    {
        try
        {
            //Zorunlu alanların kontrolü
            if (string.IsNullOrEmpty(paymentDto.Id))
                return new ApiResponse<PaymentDto> { Succeeded = false, StatusCode = 400, Message = "Payment ID is required", Result = null };

            if (paymentDto.Amount <= 0)
                return new ApiResponse<PaymentDto> { Succeeded = false, StatusCode = 400, Message = "Amount must be greater than zero", Result = null };

            if (string.IsNullOrEmpty(paymentDto.ReservationId))
                return new ApiResponse<PaymentDto> { Succeeded = false, StatusCode = 400, Message = "ReservationId is required", Result = null };

            //Ödeme Kontrolü
            var existingPayment = await _paymentRepository.GetAsync(p => p.Id == paymentDto.Id);
            if (!existingPayment.Succeeded || existingPayment.Result == null)
                return new ApiResponse<PaymentDto> { Succeeded = false, StatusCode = 404, Message = "Payment not found", Result = null };

            //Rezervasyon Kontrolü
            var reservationCheck = await _reservationRepository.GetAsync(r => r.Id == paymentDto.ReservationId);
            if (!reservationCheck.Succeeded || reservationCheck.Result == null)
                return new ApiResponse<PaymentDto> { Succeeded = false, StatusCode = 404, Message = "Reservation not found", Result = null };

            var paymentEntity = _mapper.Map<PaymentEntity>(paymentDto);
            paymentEntity.PaymentDate = existingPayment.Result.PaymentDate; //Ödeme tarihini koru
            var updateResult = await _paymentRepository.UpdateAsync(paymentEntity);
            if (!updateResult.Succeeded)
                return new ApiResponse<PaymentDto> { Succeeded = false, StatusCode = 500, Message = "Payment could not be updated", Result = null };

            var resultDto = _mapper.Map<PaymentDto>(paymentEntity);
            return new ApiResponse<PaymentDto> { Succeeded = true, StatusCode = 200, Message = "Payment updated successfully", Result = resultDto };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PaymentDto> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    public async Task<ApiResponse<bool>> DeletePaymentAsync(string paymentId)
    {
        try
        {
            //Ödeme Kontrolü
            var paymentResult = await _paymentRepository.GetAsync(p => p.Id == paymentId);
            if (!paymentResult.Succeeded || paymentResult.Result == null)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 404, Message = "Payment not found", Result = false };

            var paymentEntity = _mapper.Map<PaymentEntity>(paymentResult.Result);
            var deleteResult = await _paymentRepository.DeleteAsync(paymentEntity);
            if (!deleteResult.Succeeded)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = "Payment could not be deleted", Result = false };

            return new ApiResponse<bool> { Succeeded = true, StatusCode = 200, Message = "Payment deleted successfully", Result = true };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = false };
        }
    }

    // Ödenmemiş ödemeleri getir
    public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetPendingPaymentsAsync()
    {
        try
        {
            var result = await _paymentRepository.GetPendingPaymentsAsync();
            return new ApiResponse<IEnumerable<PaymentDto>>
            {
                Succeeded = true,
                StatusCode = result.Succeeded ? 200 : 500,
                Message = result.Succeeded ? "Pending payments retrieved successfully." : "Failed to retrieve pending payments.",
                Result = result.Result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<PaymentDto>> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }

    }

    // Müşteri bazında toplam ödemeleri getir
    public async Task<ApiResponse<decimal>> GetTotalPaymentsByCustomerAsync(string customerId)
    {
        try
        {
            if (string.IsNullOrEmpty(customerId))
                return new ApiResponse<decimal> { Succeeded = false, StatusCode = 400, Message = "Customer ID is required", Result = 0 };

            var result = await _paymentRepository.GetTotalPaymnetsByCustomerAsync(customerId);
            return new ApiResponse<decimal>
            {
                Succeeded = result.Succeeded,
                StatusCode = result.Succeeded ? 200 : 500,
                Message = result.Succeeded ? "Total payments retrieved successfully." : "Failed to retrieve total payments.",
                Result = result.Result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<decimal> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = 0 };
        }
    }
}
