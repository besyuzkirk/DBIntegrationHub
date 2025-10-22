using DBIntegrationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBIntegrationHub.Infrastructure.Persistence.EntityConfigurations;

public class ScheduledJobConfiguration : IEntityTypeConfiguration<ScheduledJob>
{
    public void Configure(EntityTypeBuilder<ScheduledJob> builder)
    {
        builder.ToTable("ScheduledJobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(j => j.Description)
            .HasMaxLength(500);

        builder.Property(j => j.CronExpression)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(j => j.IsActive)
            .IsRequired();

        builder.Property(j => j.IntegrationId);
        builder.Property(j => j.GroupId);

        builder.Property(j => j.LastRunAt);
        builder.Property(j => j.NextRunAt);
        
        builder.Property(j => j.TotalRuns)
            .IsRequired();
            
        builder.Property(j => j.SuccessfulRuns)
            .IsRequired();
            
        builder.Property(j => j.FailedRuns)
            .IsRequired();

        builder.Property(j => j.HangfireJobId)
            .HasMaxLength(100);

        builder.Property(j => j.CreatedAt)
            .IsRequired();

        builder.Property(j => j.UpdatedAt);

        // Integration relationship
        builder.HasOne(j => j.Integration)
            .WithMany()
            .HasForeignKey(j => j.IntegrationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(j => j.IsActive);
        builder.HasIndex(j => j.IntegrationId);
        builder.HasIndex(j => j.GroupId);
        builder.HasIndex(j => j.HangfireJobId);
    }
}

