namespace Domain.Dtos;

public class RoomDto
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = null!;
    public string? Description { get; set; }
    public string StatusName { get; set; } = null!;
    public DateTime LastUpdated { get; set; }
}
