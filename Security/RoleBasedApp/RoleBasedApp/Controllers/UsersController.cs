using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleBasedApp.Model;
using RoleBasedApp.Services;

namespace RoleBasedApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("role-based")]
    public IActionResult GetUserByRole()
    {
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Role, "Admin"), // Simulating an Admin role
                },
                "mock"
            )
        );

        HttpContext.User = user;

        // Perform role-based authorization manually
        if (user.IsInRole("Admin"))
        {
            return Ok(new { Message = "Access granted for Admin role." });
        }
        else
        {
            return Forbid();
        }
    }

    [HttpGet("claims-based")]
    public IActionResult GetUserByClaim()
    {
        // Simulate a logged-in user with a claim
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim("Department", "IT"), // Simulating an IT Department claim
                },
                "mock"
            )
        );

        HttpContext.User = user;

        // Perform claim-based authorization manually
        var hasClaim = user.HasClaim(c => c.Type == "Department" && c.Value == "IT");

        if (hasClaim)
        {
            return Ok(new { Message = "Access granted for IT department." });
        }
        else
        {
            return Forbid();
        }
    }

    [HttpPost("register")]
    public IActionResult Register(User user)
    {
        var authManager = new AuthManager();
        authManager.Register(user);

        return Ok(new { Message = "Registration successful." });
    }

    [HttpPost("login")]
    public IActionResult Login(User user)
    {
        var authManager = new AuthManager();
        var loggedInUser = authManager.Login(user);

        if (loggedInUser == null)
        {
            return Unauthorized(new { Message = "Invalid credentials." });
        }

        return Ok(new { Message = "Login successful.", Token = loggedInUser.Token });
    }

    [HttpPost("secure-content")]
    public IActionResult AccessSecureContent(User user)
    {
        var authManager = new AuthManager();
        var secureContentManager = new SecureContentManager(authManager);

        var result = secureContentManager.AccessSecureContent(user);

        if (result.StartsWith("Access granted"))
        {
            return Ok(new { Message = result });
        }
        else
        {
            return Forbid();
        }
    }
}
