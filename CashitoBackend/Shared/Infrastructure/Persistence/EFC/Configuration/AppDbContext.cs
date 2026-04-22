using CashitoBackend.Clients.Domain.Model.Aggregates;
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
    }
}