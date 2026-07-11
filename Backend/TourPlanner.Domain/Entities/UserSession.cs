namespace TourPlanner.Domain.Entities;

public sealed class UserSession : EntityBase
{
    public Guid UserId { get; private set; }

    public string RefreshToken { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? LastSeenAt { get; private set; }

    public User User { get; private set; } = null!;

    private UserSession()
    {
    }

    public static UserSession Create(Guid userId, string refreshToken, DateTimeOffset expiresAt)
    {
        Guard.NotEmpty(userId, nameof(userId));
        Guard.NotNullOrWhiteSpace(refreshToken, nameof(refreshToken));

        return new UserSession
        {
            UserId = userId,
            RefreshToken = refreshToken.Trim(),
            ExpiresAt = expiresAt
        };
    }

    public void Renew(string refreshToken, DateTimeOffset expiresAt)
    {
        Guard.NotNullOrWhiteSpace(refreshToken, nameof(refreshToken));
        RefreshToken = refreshToken.Trim();
        ExpiresAt = expiresAt;
        LastSeenAt = DateTimeOffset.UtcNow;
        Touch();
    }
}

