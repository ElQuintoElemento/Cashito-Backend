using CashitoBackend.IAM.Domain.Model.ValueObjects;

namespace CashitoBackend.IAM.Domain.Model.Entities;

/// <summary>
/// Entidad de dominio que representa un rol de usuario.
/// </summary>
public class Role
{
    public int Id { get; private set; }

    public Roles Name { get; private set; }

    // Constructor vacío requerido por EF Core
    private Role() { }

    public Role(Roles name)
    {
        Name = name;
    }

    /// <summary>
    /// Retorna el nombre del rol como string.
    /// </summary>
    public string GetStringName() => Name.ToString();

    /// <summary>
    /// Retorna el rol por defecto del sistema.
    /// </summary>
    public static Role GetDefaultRole() => new(Roles.User);

    /// <summary>
    /// Convierte un string a Role de forma segura.
    /// </summary>
    public static Role ToRoleFromName(string name)
    {
        if (!Enum.TryParse<Roles>(name, true, out var role))
            throw new ArgumentException($"Invalid role name: {name}");

        return new Role(role);
    }
}