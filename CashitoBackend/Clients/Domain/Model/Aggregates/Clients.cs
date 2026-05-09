using CashitoBackend.Shared.Domain.Model.ValueObjects;

namespace CashitoBackend.Clients.Domain.Model.Aggregates;

public class Client
{
    public int Id { get; private set; }

    public int UserId { get; private set; } // dueño del cliente

    public string Dni { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public decimal MonthlyIncome { get; private set; }
    
    public Currency IncomeCurrency { get; private set; } = Currency.PEN;

    public string Phone { get; private set; } = string.Empty;
    
    public EmailAddress Email { get; private set; }

    // Constructor para EF
    protected Client() { }

    // Constructor principal
    public Client(
        int userId,
        string dni,
        string firstName,
        string lastName,
        decimal monthlyIncome,
        Currency incomeCurrency,
        string phone,
        EmailAddress email)
    {
        // VALIDACIONES BÁSICAS
        if (string.IsNullOrWhiteSpace(dni))
            throw new ArgumentException("DNI es requerido");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Nombre es requerido");

        if (monthlyIncome < 0)
            throw new ArgumentException("Ingresos no pueden ser negativos");
        
        if (!Enum.IsDefined(typeof(Currency), incomeCurrency))
            throw new ArgumentException("Invalid income currency");

        UserId = userId;
        Dni = dni;
        FirstName = firstName;
        LastName = lastName;
        MonthlyIncome = monthlyIncome;
        IncomeCurrency = incomeCurrency;
        Phone = phone;
        Email = email;
    }

    public void Update(
        string name,
        string lastName,
        decimal monthlyIncome,
        Currency incomeCurrency,
        string phone,
        EmailAddress email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nombre es requerido");

        if (monthlyIncome < 0)
            throw new ArgumentException("Ingresos no pueden ser negativos");

        FirstName = name;
        LastName = lastName;
        MonthlyIncome = monthlyIncome;
        IncomeCurrency = incomeCurrency;
        Phone = phone;
        Email = email;
    }
}