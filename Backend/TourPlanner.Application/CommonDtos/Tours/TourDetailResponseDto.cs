using TourPlanner.Application.CommonDtos.TourLogs;
using TourPlanner.Domain.Enums;

namespace TourPlanner.Application.CommonDtos.Tours;

public sealed record TourDetailResponseDto(
    Guid Id,
    string Name,
    string Description,
    string From,
    string To,
    TransportType TransportType,
    double DistanceKm,
    double EstimatedMinutes,
    string RouteInformation,
    int Popularity,
    double ChildFriendliness,
    string? ImagePath,
    IReadOnlyList<TourLogResponseDto> Logs,
    string? RouteGeoJson);

