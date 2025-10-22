using DBIntegrationHub.Application.IntegrationLogs.Queries.GetAllLogs;
using DBIntegrationHub.Application.IntegrationLogs.Queries.GetLogsByIntegrationId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DBIntegrationHub.Presentation.Controllers;

public class LogsController : ApiController
{
    public LogsController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Tüm integration log'larını getirir (en son 100 kayıt)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllLogsQuery(limit);
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Belirtilen integration için log'ları getirir
    /// </summary>
    [HttpGet("integration/{integrationId:guid}")]
    public async Task<IActionResult> GetByIntegrationId(
        Guid integrationId,
        CancellationToken cancellationToken)
    {
        var query = new GetLogsByIntegrationIdQuery(integrationId);
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

