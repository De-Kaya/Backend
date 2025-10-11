using AutoMapper;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Models;
using Domain.Dtos;

namespace Business.Services;

public class CustomerService(ICustomerRepository customerRepository, IReservationRepository reservationRepository, ICustomerBalanceRepository customerBalanceRepository, IMapper mapper ) : ICustomerService
{
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly ICustomerBalanceRepository _customerBalanceRepository = customerBalanceRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerDto customerDto)
    {
        try
        {
            //Zorunlu alan kontrollu
            if (string.IsNullOrEmpty(customerDto.FullName))
                return new ApiResponse<CustomerDto> { Succeeded = false, Message = "Fullname is required.", Result = null };

            //E-posta veya telefon numarasinin benzersiz olmasi kontrolu
            if(!string.IsNullOrEmpty(customerDto.Email))
            {
                var emailCheck = await _customerRepository.GetAsync(c => c.Email == customerDto.Email);
                if (emailCheck.Succeeded && emailCheck.Result != null)
                    return new ApiResponse<CustomerDto> { Succeeded = false, Message = "A customer with the same email already exists.", Result = null };
            }

            if (!string.IsNullOrEmpty(customerDto.PhoneNumber))
            {
                var phoneCheck = await _customerRepository.GetAsync(c => c.PhoneNumber == customerDto.PhoneNumber);
                if (phoneCheck.Succeeded && phoneCheck.Result != null)
                    return new ApiResponse<CustomerDto> { Succeeded = false, Message = "A customer with the same phone number already exists.", Result = null };
            }

            var customerEntity = _mapper.Map<CustomerEntity>(customerDto);
            customerEntity.Id = Guid.NewGuid().ToString();
            customerEntity.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"));
            var addResult = await _customerRepository.AddAsync(customerEntity);
            if (!addResult.Succeeded)
                return new ApiResponse<CustomerDto> { Succeeded = false, Message = "Failed to create customer.", Result = null };

            var resultDto = _mapper.Map<CustomerDto>(customerEntity);
            return new ApiResponse<CustomerDto> { Succeeded = true, Message = "Customer created successfully.", Result = resultDto };

        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerDto> { Succeeded = false, Message = $"An error occurred while creating the customer: {ex.Message}", Result = null };
        }
    }

    public async Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(CustomerDto customerDto)
    {
        try
        {
            // Zorunlu alan kontrolü
            if (string.IsNullOrEmpty(customerDto.FullName))
                return new ApiResponse<CustomerDto> { Succeeded = false, StatusCode = 400, Message = "Full name is required", Result = null };

            // Müşteri kontrolü
            var existingCustomer = await _customerRepository.GetAsync(c => c.Id == customerDto.Id);
            if (!existingCustomer.Succeeded || existingCustomer.Result == null)
                return new ApiResponse<CustomerDto> { Succeeded = false, StatusCode = 404, Message = "Customer not found", Result = null };

            // E-posta veya telefon benzersizlik kontrolü
            if (!string.IsNullOrEmpty(customerDto.Email))
            {
                var emailCheck = await _customerRepository.GetAsync(c => c.Email == customerDto.Email && c.Id != customerDto.Id);
                if (emailCheck.Succeeded && emailCheck.Result != null)
                    return new ApiResponse<CustomerDto> { Succeeded = false, StatusCode = 400, Message = "Bu mail adresi başka bir müşteri için kullanıldı.", Result = null };
            }
            if (!string.IsNullOrEmpty(customerDto.PhoneNumber))
            {
                var phoneCheck = await _customerRepository.GetAsync(c => c.PhoneNumber == customerDto.PhoneNumber && c.Id != customerDto.Id);
                if (phoneCheck.Succeeded && phoneCheck.Result != null)
                    return new ApiResponse<CustomerDto> { Succeeded = false, StatusCode = 400, Message = "Bu telefon numarası başka bir müşteri için kullanıldı.", Result = null };
            }

            //Müşteriyi güncelle
            var customerEntity = _mapper.Map<CustomerEntity>(customerDto);
            customerEntity.CreatedAt = existingCustomer.Result.CreatedAt;
            var updateResult = await _customerRepository.UpdateAsync(customerEntity);
            if (!updateResult.Succeeded)
                return new ApiResponse<CustomerDto> { Succeeded = false, StatusCode = 500, Message = "Failed to update customer", Result = null };

            var resultDto = _mapper.Map<CustomerDto>(customerEntity);
            return new ApiResponse<CustomerDto> { Succeeded = true, StatusCode = 200, Message = "Customer updated successfully", Result = resultDto };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerDto> { Succeeded = false, Message = $"An error occurred while updating the customer, {ex.Message}", Result = null };
        }
    }

    public async Task<ApiResponse<bool>> DeleteCustomerAsync(string customerId)
    {
        try
        {
            //Müşteri kontrolü
            var customerResult = await _customerRepository.GetAsync(c => c.Id == customerId);
            if (!customerResult.Succeeded || customerResult.Result == null)
                return new ApiResponse<bool> { Succeeded = false, Message = "Customer not found.", Result = false };

            //Aktif rezervasyon kontrolü
            var reservations = await _reservationRepository.GetAllAsync(where: r => r.CustomerId == customerId && r.IsActive,
                includes: [r => r.Room, r => r.Customer]
                );
            if (reservations.Succeeded && reservations.Result!.Any())
                return new ApiResponse<bool> { Succeeded = false, Message = "Cannot delete customer with active reservations", Result = false };

            //Bakiye kontrolü
            var balanceResult = await GetCustomerBalanceAsync(customerId);
            if (!balanceResult.Succeeded)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = balanceResult.Message, Result = false };
            if (balanceResult.Result != 0)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 400, Message = "Cannot delete customer with a non-zero balance", Result = false };

            await _customerRepository.BeginTransactionAsync();
            try
            {
                //Müşteri bakiye kayıtlarını sil
                var balances = await _customerBalanceRepository.GetAllAsync(where: cb => cb.CustomerId == customerId);
                if (balances.Succeeded && balances.Result!.Any())
                {
                    foreach (var balance in balances.Result!)
                    {
                       var balanceDeleteResult = await _customerBalanceRepository.DeleteAsync(_mapper.Map<CustomerBalanceEntity>(balance));
                       if (!balanceDeleteResult.Succeeded)
                       {
                           await _customerRepository.RollbackTransactionAsync();
                           return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = $"Failed to delete customer balance: {balanceDeleteResult.Message}", Result = false };
                       }
                    }
                }

                //Müşteriyi sil
                var deleteResult = await _customerRepository.DeleteByIdAsync(customerId);
                if (!deleteResult.Succeeded)
                {
                    await _customerRepository.RollbackTransactionAsync();
                    return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = $"Failed to delete customer: {deleteResult.Message}", Result = false };
                }
                await _customerRepository.CommitTransactionAsync();
                return new ApiResponse<bool> { Succeeded = true, StatusCode = 200, Message = "Customer deleted successfully.", Result = true };
            }
            catch
            {
                await _customerRepository.RollbackTransactionAsync();
                throw;
            }

        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Succeeded = false, Message = $"An error occurred while deleting the customer: {ex.Message}", Result = false };
        }
    }

    public async Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(string customerId)
    {
        try
        {
            if (string.IsNullOrEmpty(customerId))
                return new ApiResponse<CustomerDto> { Succeeded = false, StatusCode = 400, Message = "Customer ID is required.", Result = null };

            var customerResult = await _customerRepository.GetAsync(c => c.Id == customerId);
            if (!customerResult.Succeeded || customerResult.Result == null)
                return new ApiResponse<CustomerDto> { Succeeded = false, StatusCode = 404, Message = "Customer not found.", Result = null };

            return new ApiResponse<CustomerDto> { Succeeded = true, StatusCode = 200, Message = "Customer retrieved successfully.", Result = customerResult.Result };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerDto> { Succeeded = false, Message = $"An error occurred while retrieving the customer: {ex.Message}", Result = null };
        }
    }

    public async Task<ApiResponse<IEnumerable<CustomerDto>>> GetAllCustomersAsync()
    {
        try
        {
            var result = await _customerRepository.GetAllAsync();
            return new ApiResponse<IEnumerable<CustomerDto>>
            {
                Succeeded = result.Succeeded,
                StatusCode = result.Succeeded ? 200 : 500,
                Message = result.Succeeded ? "Customers retrieved successfully." : "Failed to retrieve customers.",
                Result = result.Result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<CustomerDto>> { Succeeded = false, Message = $"An error occurred while retrieving customers: {ex.Message}", Result = null };
        }
    }

    public async Task<ApiResponse<decimal>> GetCustomerBalanceAsync(string customerId)
    {
        try
        {
            if (string.IsNullOrEmpty(customerId))
                return new ApiResponse<decimal> { Succeeded = false, StatusCode = 400, Message = "Customer ID is required.", Result = 0 };
            
            var customerResult = await _customerRepository.GetAsync(c => c.Id == customerId);
            if (!customerResult.Succeeded || customerResult.Result == null)
                return new ApiResponse<decimal> { Succeeded = false, StatusCode = 404, Message = "Customer not found.", Result = 0 };

            var balances = await _customerBalanceRepository.GetAllAsync(where: cb => cb.CustomerId == customerId);
            if (!balances.Succeeded)
                return new ApiResponse<decimal> { Succeeded = false, StatusCode = 500, Message = "Failed to retrieve customer balances.", Result = 0 };

            var balance = balances.Result!.Sum(b => b.TransactionType == TransactionType.Debt ? b.Amount : -b.Amount);
            return new ApiResponse<decimal> { Succeeded = true, StatusCode = 200, Message = "Customer balance retrieved successfully.", Result = balance };
        }
        catch (Exception ex)
        {
            return new ApiResponse<decimal> { Succeeded = false, Message = $"An error occurred while retrieving the customer balance: {ex.Message}", Result = 0 };
        }
    }

    public async Task<ApiResponse<IEnumerable<ReservationDto>>> GetCustomerReservationsAsync(string customerId)
    {
        try
        {
            if (string.IsNullOrEmpty(customerId))
                return new ApiResponse<IEnumerable<ReservationDto>> { Succeeded = false, StatusCode = 400, Message = "Customer ID is required.", Result = null };

            var customerResult = await _customerRepository.GetAsync(c => c.Id == customerId);
            if (!customerResult.Succeeded || customerResult.Result == null)
                return new ApiResponse<IEnumerable<ReservationDto>> { Succeeded = false, StatusCode = 404, Message = "Customer not found.", Result = null };

            var result = await _reservationRepository.GetAllAsync(
                where: r => r.CustomerId == customerId,
                includes: [r => r.Room, r => r.Customer]
                );

            return new ApiResponse<IEnumerable<ReservationDto>>
            {
                Succeeded = result.Succeeded,
                StatusCode = result.Succeeded ? 200 : 500,
                Message = result.Succeeded ? "Customer reservations retrieved successfully." : "Failed to retrieve customer reservations.",
                Result = result.Result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<ReservationDto>> { Succeeded = false, Message = $"An error occurred while retrieving customer reservations: {ex.Message}", Result = null };
        }
    }

    public async Task<ApiResponse<int>> GetCustomerCountAsync()
    {
        try
        {
            var result = await _customerRepository.GetCustomerCountAsync();
            return new ApiResponse<int>
            {
                Succeeded = result.Succeeded,
                StatusCode = result.Succeeded ? 200 : 500,
                Message = result.Succeeded ? "Customer count retrieved successfully." : "Failed to retrieve customer count.",
                Result = result.Result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<int> { Succeeded = false, Message = $"An error occurred while retrieving customer count: {ex.Message}", Result = 0 };
        }
    }

    public async Task<ApiResponse<IEnumerable<CustomerDto>>> GetCustomersWithOverdueBalanceAsync()
    {
        try
        {
            var result = await _customerRepository.GetCustomersWithOverdueBalanceAsync();
            return new ApiResponse<IEnumerable<CustomerDto>>
            {
                Succeeded = result.Succeeded,
                StatusCode = result.Succeeded ? 200 : 500,
                Message = result.Succeeded ? "Customers with overdue balance retrieved successfully." : "Failed to retrieve customers with overdue balance.",
                Result = result.Result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<CustomerDto>> { Succeeded = false, Message = $"An error occurred while retrieving customers with overdue balance: {ex.Message}", Result = null };
        }
    }

    public async Task<ApiResponse<int>> GetReservationCountAsync(string customerId)
    {
        try
        {
            if (string.IsNullOrEmpty(customerId))
                return new ApiResponse<int> { Succeeded = false, StatusCode = 400, Message = "Customer ID is required", Result = 0 };

            var customerResult = await _customerRepository.GetAsync(c => c.Id == customerId);
            if (!customerResult.Succeeded || customerResult.Result == null )
                return new ApiResponse<int> { Succeeded = false, StatusCode = 404, Message = "Customer not found", Result = 0 };

            var result = await _customerRepository.GetReservationCountAsync(customerId);
            return new ApiResponse<int>
            {
                Succeeded = result.Succeeded,
                StatusCode = result.Succeeded ? 200 : 500,
                Message = result.Succeeded ? result.Message : "Failed to retrieve reservation count.",
                Result = result.Result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<int> { Succeeded = false, Message = $"An error occurred while retrieving reservation count: {ex.Message}", Result = 0 };
        }
    }

    public async Task<ApiResponse<PageResult<CustomerDto>>> GetCustomersPagedAsync(int page, int pageSize, string? search)
    {
        try
        {
            if (pageSize > 100) pageSize = 100;

            var repoResult = await _customerRepository.GetPagedAsync(page, pageSize, search);
            return new ApiResponse<PageResult<CustomerDto>>
            {
                Succeeded = repoResult.Succeeded,
                StatusCode = repoResult.Succeeded ? 200 : 500,
                Message = repoResult.Succeeded ? "Paged customers retrieved successfully." : "Failed to retrieve paged customers.",
                Result = repoResult.Result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PageResult<CustomerDto>> { Succeeded = false, Message = $"An error occurred while retrieving paged customers: {ex.Message}", Result = null };
        }
    }
}
