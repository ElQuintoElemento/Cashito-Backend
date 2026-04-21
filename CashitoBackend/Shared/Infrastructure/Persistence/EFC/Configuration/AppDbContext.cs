using CashitoBackend.IAM.Domain.Model.Aggregates;
using CashitoBackend.IAM.Domain.Model.Entities;
using CashitoBackend.Shared.Domain.Model.ValueObjects;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
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

            e.HasMany(u => u.Roles)
                .WithMany()
                .UsingEntity(j => j.ToTable("user_roles"));
        });

        // =========================
        // IAM - ROLES
        // =========================
        builder.Entity<Role>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).ValueGeneratedOnAdd();

            e.Property(r => r.Name).IsRequired();
        });

        // =========================
        // OPTIONAL: soft delete
        // =========================
        // e.HasQueryFilter(u => u.IsActive);
    }
}