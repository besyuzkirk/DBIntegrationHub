using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Integrations.Commands.DeleteIntegration;

public record DeleteIntegrationCommand(Guid Id) : ICommand;

