using DBIntegrationHub.Application.Integrations.Commands.BatchRunIntegrations;
using DBIntegrationHub.Application.Integrations.Commands.CreateIntegration;
using DBIntegrationHub.Application.Integrations.Commands.DeleteIntegration;
using DBIntegrationHub.Application.Integrations.Commands.RunIntegration;
using DBIntegrationHub.Application.Integrations.Queries.GetAllIntegrations;
using DBIntegrationHub.Application.Integrations.Queries.GetIntegrationById;
using DBIntegrationHub.Application.Integrations.Queries.GetIntegrationColumns;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DBIntegrationHub.Presentation.Controllers;

public class IntegrationsController : ApiController
{
    public IntegrationsController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Tüm integration'ları listeler
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllIntegrationsQuery();
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// ID'ye göre integration bilgilerini getirir
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetIntegrationByIdQuery(id);
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Yeni bir integration oluşturur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateIntegrationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value },
            new { id = result.Value });
    }

    /// <summary>
    /// Integration'ın kaynak kolonlarını ve hedef parametrelerini getirir (mapping için)
    /// </summary>
    [HttpGet("{id:guid}/columns")]
    public async Task<IActionResult> GetColumns(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetIntegrationColumnsQuery(id);
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Integration'ı çalıştırır (source'tan veri çekip target'a yazar)
    /// </summary>
    [HttpPost("{id:guid}/run")]
    public async Task<IActionResult> Run(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new RunIntegrationCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Birden fazla integration'ı sırayla çalıştırır (TRANSACTIONAL - biri başarısız olursa hepsi rollback)
    /// </summary>
    [HttpPost("batch-run")]
    public async Task<IActionResult> BatchRun(
        [FromBody] BatchRunIntegrationsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Integration'ı siler
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteIntegrationCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return NoContent();
    }
}

