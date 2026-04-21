using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CashitoBackend.Shared.Domain.Model.ValueObjects;

public readonly record struct EmailAddress
{
    private static readonly Regex Regex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value))
            throw new ValidationException("Invalid e-mail format.");
        Value = value;
    }

    public override string ToString() => Value;
} 