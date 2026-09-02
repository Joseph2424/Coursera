using RoleBasedApp.Model;

namespace RoleBasedApp.Services;

public class AuthManager
{
    private readonly List<User> users = [];
    private readonly TokenManager tokenManager = new();

    public bool Register(User user)
    {
        if (users.Exists(u => u.Username == user.Username))
        {
            return false;
        }

        users.Add(user);

        return true;
    }

    public User? Login(User user)
    {
        var foundUser = users.Find(u => u.Username == user.Username && u.Password == user.Password);

        if (foundUser == null)
        {
            return null;
        }

        foundUser.Token = tokenManager.GenerateToken(foundUser);

        return foundUser;
    }

    public User GetUserByToken(string token)
    {
        return users.Find(u => u.Token == token);
    }
}
