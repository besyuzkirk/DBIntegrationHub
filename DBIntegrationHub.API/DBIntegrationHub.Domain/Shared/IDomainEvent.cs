namespace DBIntegrationHub.Domain.Shared;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}

