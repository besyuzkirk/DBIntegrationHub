using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Connections.Commands.TestConnection;

public record TestConnectionCommand(
    string DatabaseType,
    string ConnectionString
) : ICommand<ConnectionTestResult>;

