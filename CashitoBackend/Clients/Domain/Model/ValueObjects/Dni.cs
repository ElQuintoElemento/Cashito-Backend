namespace CashitoBackend.Clients.Domain.Model.ValueObjects;

public record Dni
{
    public string Value { get; }

    public Dni(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("DNI is required");

        if (value.Length != 8)
            throw new ArgumentException("DNI must have 8 digits");

        Value = value;
    }
}