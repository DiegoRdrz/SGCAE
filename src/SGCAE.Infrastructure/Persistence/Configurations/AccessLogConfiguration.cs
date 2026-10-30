using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCAE.Domain.Entities;

namespace SGCAE.Infrastructure.Persistence.Configurations;

public class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
{
    public void Configure(EntityTypeBuilder<AccessLog> builder)
    {
        builder.ToTable("AccessLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.CredentialCodePresented)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.MovementType)
            .IsRequired();

        builder.Property(l => l.Result)
            .IsRequired();

        builder.Property(l => l.RejectionReason)
            .HasMaxLength(300);

        builder.Property(l => l.Timestamp)
            .IsRequired();

        builder.HasIndex(l => l.Timestamp);
        builder.HasIndex(l => l.Result);
        builder.HasIndex(l => l.CredentialCodePresented);

        builder.HasOne(l => l.Employee)
            .WithMany(e => e.AccessLogs)
            .HasForeignKey(l => l.EmployeeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.Credential)
            .WithMany(c => c.AccessLogs)
            .HasForeignKey(l => l.CredentialId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.AccessPoint)
            .WithMany(p => p.AccessLogs)
            .HasForeignKey(l => l.AccessPointId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

