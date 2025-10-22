using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Mappings.Commands.SaveMappings;

public record SaveMappingsCommand(
    Guid IntegrationId,
    List<MappingItem> Mappings) : ICommand<bool>;

public record MappingItem(
    string SourceColumn,
    string TargetParameter);

