using DBIntegrationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBIntegrationHub.Infrastructure.Persistence.EntityConfigurations;

public class IntegrationLogConfiguration : IEntityTypeConfiguration<IntegrationLog>
{
    public void Configure(EntityTypeBuilder<IntegrationLog> builder)
    {
        builder.ToTable("IntegrationLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.IntegrationId)
            .IsRequired();

        builder.Property(l => l.RunDate)
            .IsRequired();

        builder.Property(l => l.Success)
            .IsRequired();

        builder.Property(l => l.Message)
            .HasMaxLength(500);

        builder.Property(l => l.RowCount)
            .IsRequired();

        builder.Property(l => l.DurationMs)
            .IsRequired();

        builder.Property(l => l.ErrorDetails)
            .HasMaxLength(2000);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.UpdatedAt);

        // Indexes
        builder.HasIndex(l => l.IntegrationId);
        builder.HasIndex(l => l.RunDate);
        builder.HasIndex(l => l.Success);

        // Relationships
        builder.HasOne(l => l.Integration)
            .WithMany()
            .HasForeignKey(l => l.IntegrationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

