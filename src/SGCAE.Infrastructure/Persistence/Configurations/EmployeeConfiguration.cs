using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCAE.Domain.Entities;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.IdentificationType)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.IdentificationNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(e => e.IdentificationNumber)
            .IsUnique();

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.JobTitle)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Department)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Ignore(e => e.FullName);

        builder.HasMany(e => e.Credentials)
            .WithOne(c => c.Employee)
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.AccessLogs)
            .WithOne(l => l.Employee)
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(e => e.Credentials)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(e => e.AccessLogs)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

