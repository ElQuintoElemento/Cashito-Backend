namespace CashitoBackend.Credits.Domain.Model.Exceptions;

public class CreditDomainException : Exception
{
    public CreditDomainException(string message) : base(message)
    {
    }
}