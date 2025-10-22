using DBIntegrationHub.Application.Users.Commands.AssignRole;
using DBIntegrationHub.Application.Users.Commands.CreateUser;
using DBIntegrationHub.Application.Users.Commands.DeleteUser;
using DBIntegrationHub.Application.Users.Commands.RemoveRole;
using DBIntegrationHub.Application.Users.Commands.ToggleUserStatus;
using DBIntegrationHub.Application.Users.Queries.GetAllRoles;
using DBIntegrationHub.Application.Users.Queries.GetAllUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DBIntegrationHub.Presentation.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : ApiController
{
    public UsersController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Tüm kullanıcıları listeler (Sadece Admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllUsersQuery();
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Tüm rolleri listeler (Sadece Admin)
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var query = new GetAllRolesQuery();
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Yeni kullanıcı oluşturur (Sadece Admin)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserCommand command,
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
    /// Kullanıcıyı siler (Sadece Admin)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Kullanıcı durumunu aktif/pasif yapar (Sadece Admin)
    /// </summary>
    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ToggleUserStatusCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Kullanıcı durumu güncellendi." });
    }

    /// <summary>
    /// Kullanıcıya rol atar (Sadece Admin)
    /// </summary>
    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRole(
        Guid id,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignRoleCommand(id, request.RoleName);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Rol başarıyla atandı." });
    }

    /// <summary>
    /// Kullanıcıdan rol çıkarır (Sadece Admin)
    /// </summary>
    [HttpDelete("{id:guid}/roles/{roleName}")]
    public async Task<IActionResult> RemoveRole(
        Guid id,
        string roleName,
        CancellationToken cancellationToken)
    {
        var command = new RemoveRoleCommand(id, roleName);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Rol başarıyla çıkarıldı." });
    }
}

public record AssignRoleRequest(string RoleName);

