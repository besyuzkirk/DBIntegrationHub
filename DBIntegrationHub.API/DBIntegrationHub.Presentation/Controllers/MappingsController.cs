using DBIntegrationHub.Application.Mappings.Commands.SaveMappings;
using DBIntegrationHub.Application.Mappings.Queries.GetMappingsByIntegrationId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DBIntegrationHub.Presentation.Controllers;

public class MappingsController : ApiController
{
    public MappingsController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Belirtilen integration için mapping'leri getirir
    /// </summary>
    [HttpGet("{integrationId:guid}")]
    public async Task<IActionResult> GetByIntegrationId(
        Guid integrationId,
        CancellationToken cancellationToken)
    {
        var query = new GetMappingsByIntegrationIdQuery(integrationId);
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Integration için mapping'leri kaydeder (eski mapping'ler silinir)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Save(
        [FromBody] SaveMappingsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { success = result.Value });
    }
}

