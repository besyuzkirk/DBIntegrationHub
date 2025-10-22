using DBIntegrationHub.Application.ScheduledJobs.Commands.CreateScheduledJob;
using DBIntegrationHub.Application.ScheduledJobs.Commands.DeleteScheduledJob;
using DBIntegrationHub.Application.ScheduledJobs.Commands.ToggleScheduledJob;
using DBIntegrationHub.Application.ScheduledJobs.Commands.TriggerScheduledJob;
using DBIntegrationHub.Application.ScheduledJobs.Queries.GetAllScheduledJobs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DBIntegrationHub.Presentation.Controllers;

[Route("api/scheduled-jobs")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ScheduledJobsController : ControllerBase
{
    protected readonly ISender Sender;

    public ScheduledJobsController(ISender sender)
    {
        Sender = sender;
    }

    /// <summary>
    /// Tüm zamanlanmış işleri listeler (Sadece Admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllScheduledJobsQuery();
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Yeni zamanlanmış iş oluşturur (Sadece Admin)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateScheduledJobCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetAll),
            new { id = result.Value },
            new { id = result.Value });
    }

    /// <summary>
    /// Zamanlanmış işi siler (Sadece Admin)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteScheduledJobCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Zamanlanmış işi aktif/pasif yapar (Sadece Admin)
    /// </summary>
    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ToggleScheduledJobCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "İş durumu güncellendi." });
    }

    /// <summary>
    /// Zamanlanmış işi şimdi çalıştırır (Sadece Admin)
    /// </summary>
    [HttpPost("{id:guid}/trigger")]
    public async Task<IActionResult> Trigger(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new TriggerScheduledJobCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "İş tetiklendi." });
    }
}

