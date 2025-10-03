using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class RoomEntity
{
    [Key]
    public int Id { get; set; }
    public string SerialNumber { get; set; } = null!;
    public string? Description { get; set; }
    public int StatusId { get; set; }
    public DateTime LastUpdated { get; set; }

    public virtual RoomStatusEntity Status { get; set; } = null!;
    public virtual ICollection<ReservationEntity> Reservations { get; set; } = [];
    public virtual ICollection<MaintenanceLogEntity> MaintenanceLogs { get; set; } = [];

}


