using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class ReservationEntity 
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(RoomEntity))]
    public int RoomId { get; set; }

    [ForeignKey(nameof(CustomerEntity))]
    public string CustomerId { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    public string? PriceDescription { get; set; }


    public virtual RoomEntity Room { get; set; } = null!;
    public virtual CustomerEntity Customer { get; set; } = null!;
    public virtual ICollection<PaymentEntity> Payments { get; set; } = [];

}


