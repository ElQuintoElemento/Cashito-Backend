using CashitoBackend.Credits.Domain.Model.Entities;

namespace CashitoBackend.Credits.Application.Internal.DTOs;

public class SimulationResult
{
    public decimal Cuota { get; set; }

    public decimal Tcea { get; set; }
    public decimal Van { get; set; }
    public decimal Tir { get; set; }

    public List<Installment> Installments { get; set; } = new();
}