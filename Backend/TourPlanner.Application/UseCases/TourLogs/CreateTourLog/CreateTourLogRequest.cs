namespace TourPlanner.Application.UseCases.TourLogs.CreateTourLog;

public sealed record CreateTourLogRequest(
    Guid TourId,
    DateTimeOffset AccomplishedAt,
    string Comment,
    Domain.TourDifficulty Difficulty,
    double TotalDistanceKm,
    double TotalTimeMinutes,
    int Rating);