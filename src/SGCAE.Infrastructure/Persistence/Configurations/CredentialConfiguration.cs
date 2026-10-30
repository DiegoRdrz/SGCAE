using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCAE.Domain.Entities;

namespace SGCAE.Infrastructure.Persistence.Configurations;

public class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.ToTable("Credentials");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.Status)
            .IsRequired();

        builder.Property(c => c.StatusChangeReason)
            .HasMaxLength(250);

        builder.HasOne(c => c.Employee)
            .WithMany(e => e.Credentials)
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.AccessLogs)
            .WithOne(l => l.Credential)
            .HasForeignKey(l => l.CredentialId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(c => c.AccessLogs)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

