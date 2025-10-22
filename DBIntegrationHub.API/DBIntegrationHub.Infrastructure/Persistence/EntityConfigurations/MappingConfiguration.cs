using DBIntegrationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBIntegrationHub.Infrastructure.Persistence.EntityConfigurations;

public class MappingConfiguration : IEntityTypeConfiguration<Mapping>
{
    public void Configure(EntityTypeBuilder<Mapping> builder)
    {
        builder.ToTable("Mappings");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.IntegrationId)
            .IsRequired();

        builder.Property(m => m.SourceColumn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.TargetParameter)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt);

        // Indexes
        builder.HasIndex(m => m.IntegrationId);

        // Relationships
        builder.HasOne(m => m.Integration)
            .WithMany()
            .HasForeignKey(m => m.IntegrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

