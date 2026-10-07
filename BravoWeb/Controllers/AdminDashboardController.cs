using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BravoWeb.Controllers;

/// <summary>
/// Admin Dashboard Controller - requires authentication
/// </summary>
[Authorize]
[Route("[controller]")]
public class AdminDashboardController : Controller
{
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(ILogger<AdminDashboardController> logger)
    {
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public IActionResult Index()
    {
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var role = User.FindFirst("Role")?.Value;

        _logger.LogInformation($"Admin dashboard accessed by: {username} ({role})");

        return View();
    }
}
