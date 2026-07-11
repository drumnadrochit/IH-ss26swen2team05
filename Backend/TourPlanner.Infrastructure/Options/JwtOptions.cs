namespace TourPlanner.Infrastructure.Options;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "TourPlanner";

    public string Audience { get; set; } = "TourPlanner.Client";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 14;
}

