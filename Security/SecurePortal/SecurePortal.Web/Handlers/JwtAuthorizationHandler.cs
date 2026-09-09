using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace SecurePortal.Web.Handlers;

public class JwtAuthorizationHandler(IHttpContextAccessor contextAccessor) : DelegatingHandler
{
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var context = _contextAccessor.HttpContext;

        if (context == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = context.GetTokenAsync("access_token").Result;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
