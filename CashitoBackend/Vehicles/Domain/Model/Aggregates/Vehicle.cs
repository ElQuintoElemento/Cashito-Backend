using CashitoBackend.Vehicles.Domain.Model.Exceptions;
using CashitoBackend.Vehicles.Domain.Model.ValueObjects;

namespace CashitoBackend.Vehicles.Domain.Model.Aggregates;


public class Vehicle
{
    public int Id { get; private set; }
    public int UserId { get; private set; }

    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;

    public decimal Price { get; private set; }
    
    public string Currency { get; private set; } = "PEN";

    public int Year { get; private set; }
    public VehicleType Type { get; private set; }

    protected Vehicle() { }

    public Vehicle(
        int userId,
        string brand,
        string model,
        decimal price,
        string currency,
        int year,
        VehicleType type)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new VehicleDomainException("Brand is required");

        if (string.IsNullOrWhiteSpace(model))
            throw new VehicleDomainException("Model is required");

        if (price <= 0)
            throw new VehicleDomainException("Price must be greater than 0");

        if (string.IsNullOrWhiteSpace(currency))
            throw new VehicleDomainException("Currency is required");

        if (year < 1900)
            throw new VehicleDomainException("Invalid year");
        
        
        UserId = userId;
        Brand = brand;
        Model = model;
        Price = price;
        Currency = currency.ToUpper();
        Year = year;
        Type = type;
    }

    public void Update(
        string brand,
        string model,
        decimal price,
        string currency,
        int year,
        VehicleType type)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new VehicleDomainException("Brand is required");

        if (string.IsNullOrWhiteSpace(model))
            throw new VehicleDomainException("Model is required");

        if (price <= 0)
            throw new VehicleDomainException("Price must be greater than 0");

        if (string.IsNullOrWhiteSpace(currency))
            throw new VehicleDomainException("Currency is required");

        if (year < 1900)
            throw new VehicleDomainException("Invalid year");
        
        Brand = brand;
        Model = model;
        Price = price;
        Currency = currency.ToUpper();
        Year = year;
        Type = type;
    }
}