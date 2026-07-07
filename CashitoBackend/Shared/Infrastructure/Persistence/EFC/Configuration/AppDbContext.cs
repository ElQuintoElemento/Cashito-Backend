using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Clients.Domain.Model.ValueObjects;
using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.Entities;
using CashitoBackend.IAM.Domain.Model.Aggregates;
using CashitoBackend.Notifications.Domain.Model.Aggregates;
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
    public DbSet<Notification> Notifications { get; set; }
    
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

            e.HasIndex(u => u.Email).IsUnique();
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
                .HasConversion(
                    dni => dni.Value,
                    value => new Dni(value)
                )
                .IsRequired()
                .HasColumnName("dni")
                .HasMaxLength(20);

            e.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(c => c.MonthlyIncome)
                .HasColumnType("decimal(18,2)");
            
            e.Property(v => v.IncomeCurrency)
                .IsRequired()
                .HasConversion<string>();

            e.Property(c => c.Phone)
                .HasConversion(
                    phone => phone != null ? phone.Value : null,
                    value => string.IsNullOrEmpty(value) ? null : new PhoneNumber(value)
                )
                .HasColumnName("phone")
                .HasMaxLength(20);
            
            e.Property(c => c.Email)
                .HasConversion(
                    email => email.Value,                 // VO → string
                    value => new EmailAddress(value)      // string → VO
                )
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();
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
                .HasConversion<string>();

            e.Property(v => v.Year);

            e.Property(v => v.Type)
                .HasConversion<string>();
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
            e.Property(v => v.Currency)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10);;
            
            e.Property(c => c.DownPayment).HasColumnType("decimal(18,2)");
            e.Property(c => c.FinancedAmount).HasColumnType("decimal(18,2)");

            e.Property(c => c.InterestRate).HasColumnType("decimal(10,4)");
            e.Property(c => c.TermMonths);

            e.Property(c => c.RateType).HasMaxLength(10);
            e.Property(c => c.GracePeriod);
            e.Property(c => c.GraceType)
                .HasConversion<string>()
                .HasMaxLength(20);

            e.Property(c => c.Insurance).HasColumnType("decimal(10,2)");

            e.Property(c => c.Tcea).HasColumnType("decimal(10,4)");
            e.Property(c => c.Van).HasColumnType("decimal(18,2)");
            e.Property(c => c.Tir).HasColumnType("decimal(10,6)");

            // Missing fields mapped in Package 3
            e.Property(c => c.InitialPaymentPercentage).HasColumnType("decimal(10,4)");
            e.Property(c => c.BalloonPaymentPercentage).HasColumnType("decimal(10,4)");
            e.Property(c => c.BalloonPaymentAmount).HasColumnType("decimal(18,2)");
            e.Property(c => c.AmortizableCapital).HasColumnType("decimal(18,2)");
            e.Property(c => c.Capitalization).HasMaxLength(50);
            e.Property(c => c.DesgravamenInsuranceRate).HasColumnType("decimal(10,6)");
            e.Property(c => c.VehicularInsuranceRate).HasColumnType("decimal(10,6)");
            e.Property(c => c.Portes).HasColumnType("decimal(18,2)");
            e.Property(c => c.DisbursementFee).HasColumnType("decimal(18,2)");
            e.Property(c => c.NotaryExpenses).HasColumnType("decimal(18,2)");
            e.Property(c => c.SoatAmount).HasColumnType("decimal(18,2)");
            e.Property(c => c.OtherExpenses).HasColumnType("decimal(18,2)");
            e.Property(c => c.DisbursementDate);
            e.Property(c => c.BaseInstallment).HasColumnType("decimal(18,2)");
            e.Property(c => c.EvaluationFee).HasColumnType("decimal(18,2)");
            e.Property(c => c.OpportunityRate).HasColumnType("decimal(10,4)");

            // 🔥 ENUM
            e.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // 🔥 TOKEN
            e.Property(c => c.PublicToken)
                .IsRequired()
                .HasMaxLength(100);

            // 🔥 INDEXES
            e.HasIndex(c => c.UserId);
            e.HasIndex(c => c.Status);

            // 🔥 RELACIÓN
            e.HasMany(c => c.Schedule)
                .WithOne()
                .HasForeignKey(i => i.CreditId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // =========================
        // INSTALLMENTS
        // =========================
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
            
            e.Property(i => i.CreditId).IsRequired();
            e.Property(i => i.IsPaid).IsRequired();
            e.Property(i => i.PaidAt);

            // Missing fields mapped in Package 3
            e.Property(i => i.BaseInstallment).HasColumnType("decimal(18,2)");
            e.Property(i => i.BeginningBalance).HasColumnType("decimal(18,2)");
            e.Property(i => i.DesgravamenInsurance).HasColumnType("decimal(18,2)");
            e.Property(i => i.VehicularInsurance).HasColumnType("decimal(18,2)");
            e.Property(i => i.Portes).HasColumnType("decimal(18,2)");
            e.Property(i => i.OtherExpenses).HasColumnType("decimal(18,2)");
            e.Property(i => i.CashFlow).HasColumnType("decimal(18,2)");
            e.Property(i => i.IsBalloon).IsRequired();
            e.Property(i => i.BalloonAmount).HasColumnType("decimal(18,2)");
        });

        // =========================
        // NOTIFICATIONS
        // =========================
        builder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Id).ValueGeneratedOnAdd();

            e.Property(n => n.UserId).IsRequired();

            e.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500);

            e.Property(n => n.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            e.Property(n => n.IsRead).IsRequired();

            e.Property(n => n.CreatedAt).IsRequired();

            e.HasIndex(n => n.UserId);
            e.HasIndex(n => new { n.UserId, n.IsRead });
        });
    }
}