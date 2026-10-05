using ClientJavaScript.Models;
using ClientJavaScript.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClientJavaScript.Controllers;

[Route("api/dashboard")]
[ApiController]
public sealed class DashboardController(IDashboardService dashboard) : ControllerBase
{
    [HttpGet("metrics")]
    public IActionResult Metrics() => Ok(dashboard.GetSnapshot().Metrics);

    [HttpGet("activity")]
    public IActionResult Activity() => Ok(dashboard.GetSnapshot().Activities);

    [HttpPost("activity")]
    public IActionResult AddActivity([FromBody] CreateActivityRequest request)
    {
        var item = dashboard.AddActivity(request.Title ?? string.Empty, request.Category ?? string.Empty);
        return item is null
            ? BadRequest(new { message = "Укажите название (3–80 символов) и категорию (2–30 символов)." })
            : CreatedAtAction(nameof(Activity), new { }, item);
    }
}
