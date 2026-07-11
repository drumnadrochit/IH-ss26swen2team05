using TourPlanner.Domain.Enums;

namespace TourPlanner.Application.CommonDtos.Tours;

public sealed record TourSummaryResponseDto(
    Guid Id,
    string Name,
    string Description,
    string From,
    string To,
    TransportType TransportType,
    double DistanceKm,
    double EstimatedMinutes,
    int Popularity,
    double ChildFriendliness,
    string? ImagePath,
    double[]? FromCoords,
    double[]? ToCoords,
    string? RouteGeoJson);

