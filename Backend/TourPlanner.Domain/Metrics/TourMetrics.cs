namespace TourPlanner.Domain.Metrics;

public sealed record TourMetrics(
    int LogCount,
    double AverageDifficulty,
    double AverageDistanceKm,
    double AverageTimeMinutes,
    int Popularity,
    double ChildFriendliness);

