using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SecurePortal.Web.Services;

public class AuthService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<JwtResponse?> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/auth/login",
            new { Email = email, Password = password }
        );

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<JwtResponse>();
    }
}
