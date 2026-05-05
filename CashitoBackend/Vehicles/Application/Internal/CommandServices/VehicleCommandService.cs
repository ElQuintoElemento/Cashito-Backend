using CashitoBackend.Shared.Domain.Repositories;
using CashitoBackend.Vehicles.Domain.Model.Aggregates;
using CashitoBackend.Vehicles.Domain.Model.Commands;
using CashitoBackend.Vehicles.Domain.Model.Exceptions;
using CashitoBackend.Vehicles.Domain.Model.ValueObjects;
using CashitoBackend.Vehicles.Domain.Repositories;
using CashitoBackend.Vehicles.Domain.Services;

namespace CashitoBackend.Vehicles.Application.Internal.CommandServices;

public class VehicleCommandService : IVehicleCommandService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VehicleCommandService(
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Vehicle> Handle(CreateVehicleCommand command, int userId)
    {
        if (!Enum.TryParse<VehicleType>(command.Type, true, out var parsedType))
            throw new VehicleDomainException("Invalid vehicle type");

        var vehicle = new Vehicle(
            userId,
            command.Brand,
            command.Model,
            command.Price,
            command.Currency,
            command.Year,
            parsedType
        );

        await _vehicleRepository.AddAsync(vehicle);
        await _unitOfWork.CompleteAsync();

        return vehicle;
    }
    
    public async Task<Vehicle?> Handle(UpdateVehicleCommand command, int userId)
    {
        var vehicle = await _vehicleRepository.FindByIdAsync(command.Id);

        if (vehicle == null)
            return null;

        if (vehicle.UserId != userId)
            throw new UnauthorizedAccessException("Not allowed");

        if (!Enum.TryParse<VehicleType>(command.Type, true, out var parsedType))
            throw new VehicleDomainException("Invalid vehicle type");

        vehicle.Update(
            command.Brand,
            command.Model,
            command.Price,
            command.Currency,
            command.Year,
            parsedType
        );

        _vehicleRepository.Update(vehicle);
        await _unitOfWork.CompleteAsync();

        return vehicle;
    }
    
    public async Task<bool> Handle(DeleteVehicleCommand command, int userId)
    {
        var vehicle = await _vehicleRepository.FindByIdAsync(command.Id);

        if (vehicle == null)
            return false;

        if (vehicle.UserId != userId)
            throw new UnauthorizedAccessException("Not allowed");

        _vehicleRepository.Remove(vehicle);
        await _unitOfWork.CompleteAsync();

        return true;
    }
}