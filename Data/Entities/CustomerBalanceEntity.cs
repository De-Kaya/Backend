using Domain.Dtos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class CustomerBalanceEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(CustomerEntity))]
    public string CustomerId { get; set; } = null!;

    public TransactionType TransactionType { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; } // İşlem miktarı
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; } // İşlem açıklaması

    [ForeignKey(nameof(ReservationEntity))]
    public string? ReservationId { get; set; } // İlgili rezervasyon (varsa)

    [ForeignKey(nameof(PaymentEntity))]
    public string? PaymentId { get; set; } // İlgili ödeme (varsa)

    public virtual CustomerEntity Customer { get; set; } = null!;
    public virtual ReservationEntity? Reservation { get; set; }
    public virtual PaymentEntity? Payment { get; set; }
}
