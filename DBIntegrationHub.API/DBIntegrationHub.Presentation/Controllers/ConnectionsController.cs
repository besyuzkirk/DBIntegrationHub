using DBIntegrationHub.Application.Connections.Commands.CreateConnection;
using DBIntegrationHub.Application.Connections.Commands.DeleteConnection;
using DBIntegrationHub.Application.Connections.Commands.PreviewQuery;
using DBIntegrationHub.Application.Connections.Commands.TestConnection;
using DBIntegrationHub.Application.Connections.Queries.GetAllConnections;
using DBIntegrationHub.Application.Connections.Queries.GetConnectionById;
using DBIntegrationHub.Application.Connections.Queries.GetConnectionSchema;
using DBIntegrationHub.Application.Connections.Queries.GetTableColumns;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DBIntegrationHub.Presentation.Controllers;

public class ConnectionsController : ApiController
{
    public ConnectionsController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Tüm veritabanı bağlantılarını listeler
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllConnectionsQuery();
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// ID'ye göre bağlantı bilgilerini getirir
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetConnectionByIdQuery(id);
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Yeni bir veritabanı bağlantısı oluşturur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateConnectionCommand command,
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
    /// Veritabanı bağlantısını test eder
    /// </summary>
    [HttpPost("test")]
    public async Task<IActionResult> TestConnection(
        [FromBody] TestConnectionCommand command,
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
    /// Bir sorguyu test eder ve ilk 100 satır sonuç döndürür
    /// </summary>
    [HttpPost("preview-query")]
    public async Task<IActionResult> PreviewQuery(
        [FromBody] PreviewQueryCommand command,
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
    /// Veritabanı bağlantısının schema bilgilerini getirir (tablolar ve kolonlar)
    /// </summary>
        [HttpGet("{id:guid}/schema")]
        public async Task<IActionResult> GetSchema(
            Guid id,
            CancellationToken cancellationToken)
        {
            var query = new GetConnectionSchemaQuery(id);
            var result = await Sender.Send(query, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }

        [HttpGet("{id:guid}/schema/{tableName}/columns")]
        public async Task<IActionResult> GetTableColumns(
            Guid id,
            string tableName,
            string schema = "dbo",
            CancellationToken cancellationToken = default)
        {
            var query = new GetTableColumnsQuery(id, tableName, schema);
            var result = await Sender.Send(query, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }

    /// <summary>
    /// Veritabanı bağlantısını siler
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteConnectionCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return NoContent();
    }
}

