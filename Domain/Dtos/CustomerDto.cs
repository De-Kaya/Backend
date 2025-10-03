using System.ComponentModel.DataAnnotations;

namespace Domain.Dtos;

public class CustomerDto
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }

}
