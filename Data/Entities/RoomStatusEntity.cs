using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class RoomStatusEntity
{
    [Key]
    public int Id { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
}


