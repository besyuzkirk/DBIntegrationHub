using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Connections.Commands.TestConnection;

public class TestConnectionCommandHandler : ICommandHandler<TestConnectionCommand, ConnectionTestResult>
{
    private readonly IConnectionTester _connectionTester;

    public TestConnectionCommandHandler(IConnectionTester connectionTester)
    {
        _connectionTester = connectionTester;
    }

    public async Task<Result<ConnectionTestResult>> Handle(
        TestConnectionCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _connectionTester.TestConnectionAsync(
            request.DatabaseType,
            request.ConnectionString,
            cancellationToken);

        return Result.Success(result);
    }
}

