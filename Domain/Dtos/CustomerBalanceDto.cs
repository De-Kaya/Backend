namespace Domain.Dtos;

public class CustomerBalanceDto
{
    public string Id { get; set; } = null!;
    public string CustomerId { get; set; } = null!;
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; } // İşlem miktarı
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; } // İşlem açıklaması
    public string? ReservationId { get; set; } // İlgili rezervasyon (varsa)
    public string? PaymentId { get; set; } // İlgili ödeme (varsa)
    public string? CustomerFullName { get; set; } // Müşteri adı
}

public enum TransactionType
{
    Debt,      // Borç: Müşteri tarafından ödenecek tutar (bakiyeyi artırır)
    Payment,   // Ödeme: Müşterinin yaptığı ödeme (bakiyeyi azaltır)
    Refund     // İade: Müşteriye yapılan geri ödeme (bakiyeyi azaltır)
}