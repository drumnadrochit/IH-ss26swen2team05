namespace TourPlanner.Domain.Entities;

public sealed class User : EntityBase
{
    public string UserName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public ICollection<Tour> Tours { get; private set; } = new List<Tour>();

    public ICollection<UserSession> Sessions { get; private set; } = new List<UserSession>();

    private User()
    {
    }

    public static User Create(string userName, string passwordHash)
    {
        Guard.NotNullOrWhiteSpace(userName, nameof(userName));
        Guard.NotNullOrWhiteSpace(passwordHash, nameof(passwordHash));

        return new User
        {
            UserName = userName.Trim(),
            PasswordHash = passwordHash.Trim()
        };
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        Guard.NotNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        PasswordHash = passwordHash.Trim();
        Touch();
    }
}

