namespace DBIntegrationHub.Application.Abstractions.Data;

public interface IQueryPreviewService
{
    Task<List<string>> GetColumnNamesAsync(
        string databaseType,
        string connectionString,
        string query,
        CancellationToken cancellationToken = default);
    
    Task<QueryPreviewData> GetPreviewDataAsync(
        string databaseType,
        string connectionString,
        string query,
        int maxRows = 100,
        CancellationToken cancellationToken = default);
    
    List<string> ExtractParametersFromQuery(string query);
}

public record QueryPreviewData(
    List<string> Columns,
    List<Dictionary<string, object?>> Rows,
    int RowCount);

