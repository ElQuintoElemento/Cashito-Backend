using System.ComponentModel.DataAnnotations;

namespace CashitoBackend.IAM.Interfaces.REST.Resources;

public class ChangePasswordResource
{
    [Required]
    public string CurrentPassword { get; set; }

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; }
}