namespace Domain.Dtos.Payment;

public class PaymentDto
{
    public string? Id { get; set; }
    public string? ReservationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public int PaymentMethod { get; set; }
    public bool IsPaid { get; set; }
}
