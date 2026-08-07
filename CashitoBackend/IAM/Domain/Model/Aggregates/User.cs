using System.Text.Json.Serialization;
using CashitoBackend.Shared.Domain.Model.Entities;
using CashitoBackend.Shared.Domain.Model.ValueObjects;

namespace CashitoBackend.IAM.Domain.Model.Aggregates;

/// <summary>
/// Aggregate de usuario del sistema.
/// </summary>
public class User : AuditableAggregateRoot
{
    public int Id { get; private set; }

    public string Username { get; private set; }

    [JsonIgnore]
    public string PasswordHash { get; private set; }

    public EmailAddress? Email { get; private set; }

    public string? FirstName { get; private set; }

    public string? LastName { get; private set; }

    public bool Active { get; private set; } = true;

    // Constructor para EF
    private User() { }

    public User(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;
    }

    public User UpdateUsername(string username)
    {
        Username = username;
        return this;
    }

    public User UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        return this;
    }

    public User UpdatePersonalInfo(string? firstName, string? lastName, EmailAddress? email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        return this;
    }

    public User Activate()
    {
        Active = true;
        return this;
    }

    public User Deactivate()
    {
        Active = false;
        return this;
    }
    
}