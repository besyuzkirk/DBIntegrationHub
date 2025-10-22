using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Connections.Commands.DeleteConnection;

public record DeleteConnectionCommand(Guid Id) : ICommand;

