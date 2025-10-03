namespace Domain.Dtos.Payment;

public class CreatePaymentResponseDto
{
    public string? Id { get; set; }
    public string? ReservationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public int PaymentMethodId { get; set; }
    public bool IsPaid { get; set; }
}
