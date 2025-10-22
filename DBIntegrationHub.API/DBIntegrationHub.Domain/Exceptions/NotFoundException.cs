namespace DBIntegrationHub.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} bulunamadı. Id: {key}")
    {
    }
}

