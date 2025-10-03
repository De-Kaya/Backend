using Business.Interfaces;
using Domain.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerController(ICustomerService customerService) : ControllerBase
{
    private readonly ICustomerService _customerService = customerService;

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CustomerDto customerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Message = "Invalid customer data." });

        var result = await _customerService.CreateCustomerAsync(customerDto);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return StatusCode(result.StatusCode, result.Result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(string id, [FromBody] CustomerDto customerDto)
    {
        if (!ModelState.IsValid || id != customerDto.Id)
            return BadRequest(new { Message = "Invalid request: ID in URL must match ID in body." });

        var result = await _customerService.UpdateCustomerAsync(customerDto);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return StatusCode(result.StatusCode, result.Result);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(string id)
    {
        var result = await _customerService.DeleteCustomerAsync(id);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return StatusCode(result.StatusCode, new { result.Message });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(string id)
    {
        var result = await _customerService.GetCustomerByIdAsync(id);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }

    [HttpGet("all-customer")]
    public async Task<IActionResult> GetAllCustomers()
    {
        var result = await _customerService.GetAllCustomersAsync();
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetCustomerBalance(string id)
    {
        var result = await _customerService.GetCustomerBalanceAsync(id);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }

    [HttpGet("{id}/reservations")]
    public async Task<IActionResult> GetCustomerReservations(string id)
    {
        var result = await _customerService.GetCustomerReservationsAsync(id);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCustomerCount()
    {
        var result = await _customerService.GetCustomerCountAsync();
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }

    [HttpGet("{id}/reservation-count")]
    public async Task<IActionResult> GetReservationCount(string id)
    {
        var result = await _customerService.GetReservationCountAsync(id);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> GetCustomersWithOverdueBalance()
    {
        var result = await _customerService.GetCustomersWithOverdueBalanceAsync();
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }
}
