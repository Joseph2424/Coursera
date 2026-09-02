using System.Security.Claims;
using ExternalAuth.Models;
using ExternalAuth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExternalAuth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(JwtTokenService jwtService) : ControllerBase
{
    private readonly JwtTokenService _jwtService = jwtService;

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback)),
        };

        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync();

        if (!authenticateResult.Succeeded)
        {
            return Unauthorized();
        }

        var email = User.FindFirstValue(ClaimTypes.Email);
        var name = User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        var jwt = _jwtService.GenerateToken(email, name ?? string.Empty);

        return Ok(
            new AuthResponse
            {
                AccessToken = jwt,
                Email = email,
                Name = name ?? "",
            }
        );
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(
            new { Name = User.Identity?.Name, Email = User.FindFirstValue(ClaimTypes.Email) }
        );
    }
}
