using DBIntegrationHub.Application.Auth.Commands.ChangePassword;
using DBIntegrationHub.Application.Auth.Commands.Login;
using DBIntegrationHub.Application.Auth.Commands.Register;
using DBIntegrationHub.Application.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DBIntegrationHub.Presentation.Controllers;

public class AuthController : ApiController
{
    public AuthController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Kullanıcı girişi yapar ve JWT token döndürür
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Yeni kullanıcı kaydı oluşturur
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetCurrentUser),
            new { id = result.Value });
    }

    /// <summary>
    /// Kullanıcı şifresini değiştirir
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Şifre başarıyla değiştirildi." });
    }

    /// <summary>
    /// Giriş yapmış kullanıcının bilgilerini getirir
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var query = new GetCurrentUserQuery();
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

