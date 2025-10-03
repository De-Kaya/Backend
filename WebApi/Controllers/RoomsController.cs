using Business.Interfaces;
using Domain.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomsController(IRoomService roomService) : ControllerBase
{
    private readonly IRoomService _roomService = roomService;

    [HttpGet]
    public async Task<IActionResult> GetAllRooms()
    {
        var result = await _roomService.GetAllRoomsAsync();
        if (!result.Succeeded)
            return BadRequest(result.Message);

        return Ok(result.Result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] RoomDto roomDto)
    {
        var result = await _roomService.CreateRoomAsync(roomDto);
        if (!result.Succeeded)
            return BadRequest(result.Message);
        return Ok(result.Result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRoom([FromBody] RoomDto roomDto)
    {
        var result = await _roomService.UpdateRoomAsync(roomDto);
        if (!result.Succeeded)
            return BadRequest(result.Message);
        return Ok(result.Result);
    }

    [HttpDelete("{roomId}")]
    public async Task<IActionResult> DeleteRoom(int roomId)
    {
        var result = await _roomService.DeleteRoomAsync(roomId);
        if (!result.Succeeded)
            return BadRequest(result.Message);
        return Ok(result.Result);
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableRooms()
    {
        var result = await _roomService.GetAvailableRoomsAsync();
        if (!result.Succeeded)
            return BadRequest(result.Message);
        return Ok(result.Result);
    }

    [HttpGet("status/{statusName}")]
    public async Task<IActionResult> GetRoomsByStatus(string statusName)
    {
        var result = await _roomService.GetRoomsByStatusAsync(statusName);
        if (!result.Succeeded)
            return BadRequest(result.Message);
        return Ok(result.Result);
    }

    [HttpPut("status/{roomId}")]
    public async Task<IActionResult> UpdateRoomStatus(int roomId, [FromBody] string statusName)
    {
        var result = await _roomService.UpdateRoomStatusAsync(roomId, statusName);
        if (!result.Succeeded)
            return BadRequest(result.Message);
        return Ok(result.Result);
    }
}
