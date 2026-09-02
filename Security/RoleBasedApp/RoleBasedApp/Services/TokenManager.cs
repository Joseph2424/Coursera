namespace RoleBasedApp.Services;

using System;
using System.Text;
using RoleBasedApp.Model;

public class TokenManager
{
    public string GenerateToken(User user)
    {
        var expiry = DateTime.UtcNow.AddMinutes(30).ToString();
        string tokenData = $"{user.Username}:{expiry}";
        
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenData));
    }
}
