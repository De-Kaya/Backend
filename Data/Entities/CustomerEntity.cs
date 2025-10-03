using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class CustomerEntity 
{ 
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ReservationEntity> Reservations { get; set; } = [];
    public virtual ICollection<CustomerBalanceEntity> CustomerBalances { get; set; } = [];
}


