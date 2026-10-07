using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BravoWeb.Models;
using BravoWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BravoWeb.Controllers;

[Route("[controller]")]
public class AuthController : Controller
{
    private readonly UserAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Display login form
    /// </summary>
    [HttpGet("login")]
    [HttpGet("signin")]
    public IActionResult Login(string? returnUrl = null)
    {
        // If already logged in, redirect to admin dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "AdminDashboard");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Handle login form submission
    /// </summary>
    [HttpPost("login")]
    [HttpPost("signin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Authenticate user
        var user = await _authService.AuthenticateAsync(model.Username, model.Password);

        if (user == null)
        {
            _logger.LogWarning($"Failed login attempt for username: {model.Username}");
            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        // Create authentication claims
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("Role", user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Sign in user
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });

        _logger.LogInformation($"User logged in: {user.Username}");

        // Redirect to return URL or admin dashboard
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "AdminDashboard");
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [HttpPost("logout")]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation($"User logged out: {username}");

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Access denied page
    /// </summary>
    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

/// <summary>
/// View model for login form
/// </summary>
public class LoginViewModel
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; }
}
