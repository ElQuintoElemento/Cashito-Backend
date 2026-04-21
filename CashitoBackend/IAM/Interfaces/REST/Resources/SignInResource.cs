using System.ComponentModel.DataAnnotations;

namespace CashitoBackend.IAM.Interfaces.REST.Resources;

public class SignInResource
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}