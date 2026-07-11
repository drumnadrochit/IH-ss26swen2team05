namespace TourPlanner.Application.CommonDtos.TourLogs;

public sealed record TourLogResponseDto(
    Guid Id,
    Guid TourId,
    DateTimeOffset AccomplishedAt,
    string Comment,
    TourPlanner.Domain.TourDifficulty Difficulty,
    double TotalDistanceKm,
    double TotalTimeMinutes,
    int Rating);


