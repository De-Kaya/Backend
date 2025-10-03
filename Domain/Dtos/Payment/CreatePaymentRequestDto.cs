namespace Domain.Dtos.Payment;

public class CreatePaymentRequestDto
{
    public string? ReservationId { get; set; }
    public decimal Amount { get; set; }
    public int PaymentMethodId { get; set; }
    public bool IsPaid { get; set; }
}
