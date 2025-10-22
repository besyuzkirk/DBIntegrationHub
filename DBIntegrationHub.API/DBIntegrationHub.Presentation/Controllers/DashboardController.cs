using DBIntegrationHub.Application.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DBIntegrationHub.Presentation.Controllers;

[Route("api/[controller]")]
public class DashboardController : ApiController
{
    public DashboardController(ISender sender) : base(sender)
    {
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var query = new GetDashboardStatisticsQuery();
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("recent-logs")]
    public async Task<IActionResult> GetRecentLogs(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRecentIntegrationLogsQuery(count);
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        var query = new GetIntegrationActivityQuery(days);
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

