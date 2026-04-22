using CashitoBackend.Vehicles.Domain.Model.Exceptions;

namespace CashitoBackend.Vehicles.Domain.Model.ValueObjects;

public record Price
{
    public decimal Value { get; }
    
    public Price(decimal value)
    {
        if (Value <= 0)
            throw new VehicleDomainException("Price must be greater than 0");

        Value = value;
    }
}


