namespace SimpleBlog.ApiService;

internal sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, (string Password, string Role)> _users = new()
    {
        [SeedDataConstants.AdminUsername.ToLower()] = ("admin123", SeedDataConstants.AdminUsername),
        ["user"] = ("user123", "User")
    };

    public User? ValidateUser(string username, string password)
    {
        if (_users.TryGetValue(username, out var userInfo) && userInfo.Password == password)
        {
            return new User(username, string.Empty, userInfo.Role);
        }
        return null;
    }
}
