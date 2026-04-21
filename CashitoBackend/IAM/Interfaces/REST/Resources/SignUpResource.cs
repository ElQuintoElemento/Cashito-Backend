namespace CashitoBackend.IAM.Interfaces.REST.Resources;

using System.ComponentModel.DataAnnotations;

public class SignUpResource
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    public string Username { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }
}