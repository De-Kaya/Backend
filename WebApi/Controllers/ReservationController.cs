using Business.Interfaces;
using Domain.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservationController(IReservationService reservationService) : ControllerBase
{
    private readonly IReservationService _reservationService = reservationService;


    [HttpGet]
    public async Task<IActionResult> GetAllReservations()
    {
        var result = await _reservationService.GetAllReservationAsync();
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservation(string id)
    {
        var result = await _reservationService.GetReservationByIdAsync(id);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result.Result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationDto reservationDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _reservationService.CreateReservationAsync(reservationDto);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });

        return Ok(new { result.Message, result.StatusCode, result.Result});
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReservation(string id, [FromBody] ReservationDto reservationDto)
    {
        if (!ModelState.IsValid || id != reservationDto.Id)
            return BadRequest(new { Message = "Invalid request: ID in URL must match ID in body"});
        
        var result = await _reservationService.UpdateReservationAsync(reservationDto);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReservation(string id)
    {
        var result = await _reservationService.DeleteReservationAsync(id);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveReservations()
    {
        var result = await _reservationService.GetActiveReservationsAsync();
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result);
    }

    [HttpGet("expired")]
    public async Task<IActionResult> GetExpiredReservations()
    {
        var result = await _reservationService.GetExpiredReservationsAsync();
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result);
    }

    [HttpGet("by-date-range")]
    public async Task<IActionResult> GetReservationsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _reservationService.GetReservationsByDateRangeAsync(startDate, endDate);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, new { result.Message });
        return Ok(result);
    }


}
