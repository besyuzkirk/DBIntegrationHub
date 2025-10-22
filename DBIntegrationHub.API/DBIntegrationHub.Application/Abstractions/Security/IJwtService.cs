using DBIntegrationHub.Domain.Entities;

namespace DBIntegrationHub.Application.Abstractions.Security;

public interface IJwtService
{
    string GenerateToken(User user, IEnumerable<string> roles);
    string? ValidateToken(string token);
}

