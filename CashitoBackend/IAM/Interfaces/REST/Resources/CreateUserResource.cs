using System.ComponentModel.DataAnnotations;

namespace CashitoBackend.IAM.Interfaces.REST.Resources;

public class CreateUserResource
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public IEnumerable<string> Roles { get; set; } = new List<string>();
}