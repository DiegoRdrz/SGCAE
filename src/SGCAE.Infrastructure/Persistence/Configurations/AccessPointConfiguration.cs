using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCAE.Domain.Entities;

namespace SGCAE.Infrastructure.Persistence.Configurations;

public class AccessPointConfiguration : IEntityTypeConfiguration<AccessPoint>
{
    public void Configure(EntityTypeBuilder<AccessPoint> builder)
    {
        builder.ToTable("AccessPoints");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(300);

        builder.Property(p => p.PhysicalLocation)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.IsEnabled)
            .IsRequired();

        builder.HasMany(p => p.AccessLogs)
            .WithOne(l => l.AccessPoint)
            .HasForeignKey(l => l.AccessPointId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(p => p.AccessLogs)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

