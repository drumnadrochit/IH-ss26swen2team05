namespace TourPlanner.Application.UseCases.TourLogs.UpdateTourLog;

public sealed record UpdateTourLogRequest(
    Guid TourLogId,
    DateTimeOffset AccomplishedAt,
    string Comment,
    Domain.TourDifficulty Difficulty,
    double TotalDistanceKm,
    double TotalTimeMinutes,
    int Rating);


