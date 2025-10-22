using FluentValidation;
using System.Net;
using System.Text.Json;

namespace DBIntegrationHub.Presentation.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmeyen bir hata oluştu: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        object response;
        int statusCode;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    StatusCode = statusCode,
                    Message = "Doğrulama hatası",
                    Errors = validationException.Errors.Select(e => new
                    {
                        Property = e.PropertyName,
                        Message = e.ErrorMessage
                    })
                };
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                response = new
                {
                    StatusCode = statusCode,
                    Message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
                    Error = exception.Message
                };
                break;
        }

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

