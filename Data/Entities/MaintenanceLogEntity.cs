using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class MaintenanceLogEntity
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(RoomEntity))]
    public int RoomId { get; set; }

    public string Description { get; set; } = null!;
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; } // Sorun çözüldü mü?

    public virtual RoomEntity Room { get; set; } = null!;
}


