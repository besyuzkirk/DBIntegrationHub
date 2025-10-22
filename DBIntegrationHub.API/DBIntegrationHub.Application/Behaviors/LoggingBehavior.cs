using MediatR;
using Microsoft.Extensions.Logging;

namespace DBIntegrationHub.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "İstek başlatılıyor: {RequestName} {@Request}",
            requestName,
            request);

        try
        {
            var response = await next();

            _logger.LogInformation(
                "İstek tamamlandı: {RequestName}",
                requestName);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "İstek başarısız: {RequestName}",
                requestName);

            throw;
        }
    }
}

