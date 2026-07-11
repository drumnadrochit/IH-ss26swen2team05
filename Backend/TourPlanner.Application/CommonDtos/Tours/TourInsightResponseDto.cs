namespace TourPlanner.Application.CommonDtos.Tours;

public sealed record TourInsightResponseDto(
    Guid TourId,
    int LogCount,
    double AverageDifficulty,
    double AverageDistanceKm,
    double AverageTimeMinutes,
    int Popularity,
    double ChildFriendliness);

