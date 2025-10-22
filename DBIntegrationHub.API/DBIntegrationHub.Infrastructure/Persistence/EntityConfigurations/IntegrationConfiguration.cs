using DBIntegrationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBIntegrationHub.Infrastructure.Persistence.EntityConfigurations;

public class IntegrationConfiguration : IEntityTypeConfiguration<Integration>
{
    public void Configure(EntityTypeBuilder<Integration> builder)
    {
        builder.ToTable("Integrations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.SourceConnectionId)
            .IsRequired();

        builder.Property(i => i.TargetConnectionId)
            .IsRequired();

        builder.Property(i => i.SourceQuery)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(i => i.TargetQuery)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(i => i.GroupName)
            .HasMaxLength(100);

        builder.Property(i => i.ExecutionOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.UpdatedAt);

        // Relationships
        builder.HasOne(i => i.SourceConnection)
            .WithMany()
            .HasForeignKey(i => i.SourceConnectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.TargetConnection)
            .WithMany()
            .HasForeignKey(i => i.TargetConnectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.Name);
        builder.HasIndex(i => i.SourceConnectionId);
        builder.HasIndex(i => i.TargetConnectionId);
        builder.HasIndex(i => i.GroupName);
        builder.HasIndex(i => new { i.GroupName, i.ExecutionOrder });
    }
}

