namespace CashitoBackend.Clients.Domain.Model.Exceptions;

public class InvalidClienteDataException : Exception
{
    public InvalidClienteDataException(string message) : base(message) { }
}