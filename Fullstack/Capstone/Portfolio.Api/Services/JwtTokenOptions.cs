namespace Portfolio.Api.Services;

public sealed class JwtTokenOptions
{
    public string Issuer { get; set; } = "Portfolio.Api";

    public string Audience { get; set; } = "Portfolio.Client";

    public string Key { get; set; } = string.Empty;

    public int LifetimeMinutes { get; set; } = 60;
}