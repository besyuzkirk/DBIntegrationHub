using DBIntegrationHub.Domain.Shared;
using MediatR;

namespace DBIntegrationHub.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

