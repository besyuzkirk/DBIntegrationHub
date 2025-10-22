using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Domain.Entities;

public class Mapping : Entity
{
    public Guid IntegrationId { get; private set; }
    public string SourceColumn { get; private set; }
    public string TargetParameter { get; private set; }
    
    // Navigation properties
    public Integration? Integration { get; private set; }

    // Private constructor for EF Core
    private Mapping()
    {
        SourceColumn = string.Empty;
        TargetParameter = string.Empty;
    }

    public Mapping(
        Guid integrationId,
        string sourceColumn,
        string targetParameter)
    {
        IntegrationId = integrationId;
        SourceColumn = sourceColumn;
        TargetParameter = targetParameter;
    }

    public void Update(string sourceColumn, string targetParameter)
    {
        SourceColumn = sourceColumn;
        TargetParameter = targetParameter;
        UpdatedAt = DateTime.UtcNow;
    }
}

