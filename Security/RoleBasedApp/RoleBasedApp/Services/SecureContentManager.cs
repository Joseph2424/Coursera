using RoleBasedApp.Model;

namespace RoleBasedApp.Services;

public class SecureContentManager(AuthManager authManager)
{
    private readonly AuthManager authManager = authManager;

    public string AccessSecureContent(User user)
    {
        if(user == null || string.IsNullOrEmpty(user.Token))
        {
            return "Access denied. No token provided.";
        }   

        var result = authManager.GetUserByToken(user.Token);

        if (result != null)
        {
           return $"Access granted to secure content for user: {user.Username}";
        }
        else
        {
            return "Access denied. Invalid token.";
        }
    }
}
