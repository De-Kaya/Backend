using System.ComponentModel.DataAnnotations;

namespace Domain.Dtos;

public class ReservationDto
{
    public string? Id { get; set; }
    public int RoomId { get; set; }
    public string CustomerId { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    public string? PriceDescription { get; set; }
}
