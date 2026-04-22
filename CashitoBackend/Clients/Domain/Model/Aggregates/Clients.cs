namespace CashitoBackend.Clients.Domain.Model.Aggregates;

public class Client
{
    public int Id { get; private set; }

    public int UserId { get; private set; } // dueño del cliente

    public string Dni { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public decimal MonthlyIncome { get; private set; }

    public string Phone { get; private set; } = string.Empty;

    // Constructor para EF
    protected Client() { }

    // Constructor principal
    public Client(
        int userId,
        string dni,
        string name,
        string lastName,
        decimal monthlyIncome,
        string phone)
    {
        // VALIDACIONES BÁSICAS
        if (string.IsNullOrWhiteSpace(dni))
            throw new ArgumentException("DNI es requerido");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nombre es requerido");

        if (monthlyIncome < 0)
            throw new ArgumentException("Ingresos no pueden ser negativos");

        UserId = userId;
        Dni = dni;
        Name = name;
        LastName = lastName;
        MonthlyIncome = monthlyIncome;
        Phone = phone;
    }

    // 🔥 MÉTODO DE ACTUALIZACIÓN (clave en DDD)
    public void Update(
        string name,
        string lastName,
        decimal monthlyIncome,
        string phone
        )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nombre es requerido");

        if (monthlyIncome < 0)
            throw new ArgumentException("Ingresos no pueden ser negativos");

        Name = name;
        LastName = lastName;
        MonthlyIncome = monthlyIncome;
        Phone = phone;
    }
}