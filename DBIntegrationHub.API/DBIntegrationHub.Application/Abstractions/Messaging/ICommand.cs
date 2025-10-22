using DBIntegrationHub.Domain.Shared;
using MediatR;

namespace DBIntegrationHub.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

