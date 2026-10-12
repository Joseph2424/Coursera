using Microsoft.IdentityModel.Tokens;

namespace LogiTrack.Security;

public sealed record JwtTokenSettings(
    SigningCredentials SigningCredentials,
    string Issuer,
    string Audience,
    int AccessTokenMinutes);