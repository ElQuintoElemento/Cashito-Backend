namespace CashitoBackend.IAM.Application.Internal.OutboundServices;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}
