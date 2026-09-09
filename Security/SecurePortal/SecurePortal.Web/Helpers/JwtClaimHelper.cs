using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SecurePortal.Web.Helpers;

public static class JwtClaimHelper
{
    public static ClaimsPrincipal? GetClaimsPrincipalFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(jwt))
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var token = handler.ReadJwtToken(jwt);
        var claimsIdentity = new ClaimsIdentity(token.Claims, "jwt");

        return new ClaimsPrincipal(claimsIdentity);
    }
}