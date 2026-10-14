using System.ComponentModel.DataAnnotations;

namespace Portfolio.Api.Contracts;

public sealed class RegisterRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 10)]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 10)]
    public string Password { get; set; } = string.Empty;
}

public sealed record AuthenticatedUserDto(string Id, string Email);

public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, AuthenticatedUserDto User);