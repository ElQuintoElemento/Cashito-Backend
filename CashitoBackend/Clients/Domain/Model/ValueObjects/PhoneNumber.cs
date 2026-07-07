using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CashitoBackend.Clients.Domain.Model.ValueObjects;

public record PhoneNumber
{
    private static readonly Regex Regex = new(@"^\+51\s?9\d{8}$", RegexOptions.Compiled);

    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value))
            throw new ValidationException("Invalid phone number format. It must follow +51 9XXXXXXXX.");

        Value = value;
    }

    public override string ToString() => Value;
}
