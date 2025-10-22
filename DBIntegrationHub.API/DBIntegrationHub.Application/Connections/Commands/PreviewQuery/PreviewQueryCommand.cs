using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Connections.Commands.PreviewQuery;

public record PreviewQueryCommand(
    Guid ConnectionId,
    string Query
) : ICommand<QueryPreviewResult>;

public record QueryPreviewResult(
    List<string> Columns,
    List<Dictionary<string, object?>> Rows,
    int RowCount);

