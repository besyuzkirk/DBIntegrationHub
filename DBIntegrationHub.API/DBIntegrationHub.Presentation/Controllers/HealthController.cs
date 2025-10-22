using Microsoft.AspNetCore.Mvc;

namespace DBIntegrationHub.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "DBIntegrationHub API"
        });
    }

    [HttpGet("ready")]
    public IActionResult Ready()
    {
        // Burada database bağlantısı vb. kontrol edilebilir
        return Ok(new
        {
            status = "ready",
            timestamp = DateTime.UtcNow
        });
    }
}
