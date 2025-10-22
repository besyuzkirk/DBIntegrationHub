using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Connections.Commands.CreateConnection;

public record CreateConnectionCommand(
    string Name,
    string ConnectionString,
    string DatabaseType
) : ICommand<Guid>;

