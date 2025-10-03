namespace Domain.Dtos;

public class MaintenancelLogDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string Description { get; set; } = null!;
    public DateTime LoggedAt { get; set; }
    public bool IsResolved { get; set; }
}
