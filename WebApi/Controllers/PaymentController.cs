using Business.Interfaces;
using Domain.Dtos.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    private readonly IPaymentService _paymentService = paymentService;

    [HttpGet("all-payment")]
    public async Task<IActionResult> GetAllPayments()
    {
        var result = await _paymentService.GetAllPaymentsAsync();
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });

        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentById(string id)
    {
        var result = await _paymentService.GetPaymentByIdAsync(id);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDto paymentRequestDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Message = "Invalid payment data." });

        var result = await _paymentService.CreatePaymentAsync(paymentRequestDto);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });

        return StatusCode(result.StatusCode, result.Result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(string id, [FromBody] PaymentDto paymentDto)
    {
        if (!ModelState.IsValid || id != paymentDto.Id)
            return BadRequest(new { Message = "Invalid request: ID in URL must match ID in body." });
     
        var result = await _paymentService.UpdatePaymentAsync(paymentDto);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return StatusCode(result.StatusCode, result.Result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayment(string id)
    {
        var result = await _paymentService.DeletePaymentAsync(id);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return StatusCode(result.StatusCode, new { result.Message });
    }

    [HttpGet("{customerId}/total")]
    public async Task<IActionResult> GetTotalPaymentsByCustomer(string customerId)
    {
        var result = await _paymentService.GetTotalPaymentsByCustomerAsync(customerId);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingPayments()
    {
        var result = await _paymentService.GetPendingPaymentsAsync();
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }
}
