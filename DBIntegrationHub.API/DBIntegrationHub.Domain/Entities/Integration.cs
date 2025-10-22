using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Domain.Entities;

public class Integration : Entity
{
    public string Name { get; private set; } = string.Empty;
    public Guid SourceConnectionId { get; private set; }
    public Guid TargetConnectionId { get; private set; }
    public string SourceQuery { get; private set; } = string.Empty;
    public string TargetQuery { get; private set; } = string.Empty;
    public string? GroupName { get; private set; }
    public int ExecutionOrder { get; private set; }
    
    // Navigation properties
    public Connection? SourceConnection { get; private set; }
    public Connection? TargetConnection { get; private set; }

    private Integration() { } // EF Core için

    private Integration(
        string name,
        Guid sourceConnectionId,
        Guid targetConnectionId,
        string sourceQuery,
        string targetQuery,
        string? groupName = null,
        int executionOrder = 0)
    {
        Name = name;
        SourceConnectionId = sourceConnectionId;
        TargetConnectionId = targetConnectionId;
        SourceQuery = sourceQuery;
        TargetQuery = targetQuery;
        GroupName = groupName;
        ExecutionOrder = executionOrder;
    }

    public static Integration Create(
        string name,
        Guid sourceConnectionId,
        Guid targetConnectionId,
        string sourceQuery,
        string targetQuery,
        string? groupName = null,
        int executionOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Integration adı boş olamaz", nameof(name));

        if (sourceConnectionId == Guid.Empty)
            throw new ArgumentException("Kaynak bağlantı ID'si geçersiz", nameof(sourceConnectionId));

        if (targetConnectionId == Guid.Empty)
            throw new ArgumentException("Hedef bağlantı ID'si geçersiz", nameof(targetConnectionId));

        if (string.IsNullOrWhiteSpace(sourceQuery))
            throw new ArgumentException("Kaynak sorgu boş olamaz", nameof(sourceQuery));

        if (string.IsNullOrWhiteSpace(targetQuery))
            throw new ArgumentException("Hedef sorgu boş olamaz", nameof(targetQuery));

        return new Integration(name, sourceConnectionId, targetConnectionId, sourceQuery, targetQuery, groupName, executionOrder);
    }

    public void Update(
        string name,
        Guid sourceConnectionId,
        Guid targetConnectionId,
        string sourceQuery,
        string targetQuery,
        string? groupName = null,
        int executionOrder = 0)
    {
        Name = name;
        SourceConnectionId = sourceConnectionId;
        TargetConnectionId = targetConnectionId;
        SourceQuery = sourceQuery;
        TargetQuery = targetQuery;
        GroupName = groupName;
        ExecutionOrder = executionOrder;
    }
}

