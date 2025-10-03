using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class PaymentEntity 
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(ReservationEntity))]
    public string ReservationId { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(PaymentMethodEntity))]
    public int PaymentMethodId { get; set; }

    public bool IsPaid { get; set; }

    public virtual PaymentMethodEntity PaymentMethod { get; set; } = null!;
    public virtual ReservationEntity Reservation { get; set; } = null!;
}