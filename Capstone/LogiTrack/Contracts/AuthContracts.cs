using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Contracts;

/// <summary>Credentials for creating a standard user account.</summary>
public sealed record RegisterRequest
{
    [Required, EmailAddress, MaxLength(254)]
    public required string Email { get; init; }

    [Required, MinLength(12), MaxLength(128)]
    public required string Password { get; init; }
}

/// <summary>Credentials for signing in.</summary>
public sealed record LoginRequest
{
    [Required, EmailAddress, MaxLength(254)]
    public required string Email { get; init; }

    [Required, MinLength(1), MaxLength(128)]
    public required string Password { get; init; }
}

/// <summary>Account information returned after registration.</summary>
public sealed record RegistrationResponse(string UserId, string Email);

/// <summary>Bearer token returned after successful login.</summary>
public sealed record LoginResponse(string AccessToken, string TokenType, DateTimeOffset ExpiresAt);