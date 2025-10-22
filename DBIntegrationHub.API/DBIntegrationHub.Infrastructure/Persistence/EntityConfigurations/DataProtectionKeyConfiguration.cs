using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBIntegrationHub.Infrastructure.Persistence.EntityConfigurations;

public class DataProtectionKeyConfiguration : IEntityTypeConfiguration<DataProtectionKey>
{
    public void Configure(EntityTypeBuilder<DataProtectionKey> builder)
    {
        builder.ToTable("DataProtectionKeys");
        
        builder.HasKey(k => k.Id);
        
        builder.Property(k => k.FriendlyName)
            .HasMaxLength(200);
        
        builder.Property(k => k.Xml)
            .IsRequired();
    }
}

