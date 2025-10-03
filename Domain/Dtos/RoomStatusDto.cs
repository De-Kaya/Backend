namespace Domain.Dtos;

public class RoomStatusDto
{
    public int Id { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}
