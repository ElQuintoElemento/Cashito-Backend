using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.Entities;
using CashitoBackend.IAM.Domain.Model.Aggregates;
using CashitoBackend.Shared.Domain.Model.ValueObjects;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using CashitoBackend.Vehicles.Domain.Model.Aggregates;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    // DbSets
    public DbSet<Client> Clients { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Credit> Credits { get; set; }
    public DbSet<Installment> Installments { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.UseSnakeCaseNamingConvention();

        // =========================
        // IAM - USERS
        // =========================
        builder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).ValueGeneratedOnAdd();

            e.Property(u => u.Username).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();

            e.Property(u => u.Email)
                .HasConversion(
                    v => v.HasValue ? v.Value.Value : null,
                    v => string.IsNullOrEmpty(v) ? null : new EmailAddress(v!)
                )
                .HasColumnName("email")
                .HasMaxLength(255);
        });
        
        // OPTIONAL: soft delete
        // e.HasQueryFilter(u => u.IsActive);
        
        
        // =========================
        // CLIENTS
        // =========================
        
        builder.Entity<Client>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).ValueGeneratedOnAdd();

            e.Property(c => c.UserId)
                .IsRequired();

            e.Property(c => c.Dni)
                .IsRequired()
                .HasMaxLength(20);

            e.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(c => c.MonthlyIncome)
                .HasColumnType("decimal(18,2)");

            e.Property(c => c.Phone)
                .HasMaxLength(20);
        });
        
        
        // =========================
        // VEHICLES
        // =========================
        
        builder.Entity<Vehicle>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Id).ValueGeneratedOnAdd();

            e.Property(v => v.UserId).IsRequired();

            e.Property(v => v.Brand)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(v => v.Model)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(v => v.Price)
                .HasColumnType("decimal(18,2)");

            e.Property(v => v.Currency)
                .IsRequired()
                .HasMaxLength(10);

            e.Property(v => v.Year);

            e.Property(v => v.Type)
                .IsRequired()
                .HasMaxLength(50);
        });
        
        // =========================
        // CREDITS
        // =========================
        
        builder.Entity<Credit>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).ValueGeneratedOnAdd();

            e.Property(c => c.UserId).IsRequired();
            e.Property(c => c.ClientId).IsRequired();
            e.Property(c => c.VehicleId).IsRequired();

            e.Property(c => c.VehiclePrice).HasColumnType("decimal(18,2)");
            e.Property(c => c.DownPayment).HasColumnType("decimal(18,2)");
            e.Property(c => c.FinancedAmount).HasColumnType("decimal(18,2)");

            e.Property(c => c.InterestRate).HasColumnType("decimal(10,4)");
            e.Property(c => c.TermMonths);

            e.Property(c => c.RateType).HasMaxLength(10);
            e.Property(c => c.GracePeriod);

            e.Property(c => c.Insurance).HasColumnType("decimal(10,2)");

            e.Property(c => c.Tcea).HasColumnType("decimal(10,4)");
            e.Property(c => c.Van).HasColumnType("decimal(18,2)");
            e.Property(c => c.Tir).HasColumnType("decimal(10,6)");

            // 🔥 RELACIÓN CON INSTALLMENTS
            e.HasMany(c => c.Schedule)
                .WithOne()
                .HasForeignKey("CreditId")
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<Installment>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).ValueGeneratedOnAdd();

            e.Property(i => i.Number);
            e.Property(i => i.Date);

            e.Property(i => i.TotalPayment).HasColumnType("decimal(18,2)");
            e.Property(i => i.Interest).HasColumnType("decimal(18,2)");
            e.Property(i => i.Amortization).HasColumnType("decimal(18,2)");
            e.Property(i => i.RemainingBalance).HasColumnType("decimal(18,2)");
        });
    }
}