namespace CashitoBackend.Vehicles.Domain.Model.Exceptions;

public class VehicleDomainException : Exception
{
    public VehicleDomainException(string message) : base(message)
    {
    }
}