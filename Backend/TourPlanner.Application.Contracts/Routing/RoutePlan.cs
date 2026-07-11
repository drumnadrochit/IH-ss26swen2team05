namespace TourPlanner.Application.Contracts.Routing;

public sealed record RoutePlan(
    double DistanceKm,
    double EstimatedMinutes,
    string RouteInformation,
    string GeoJson,
    double FromLatitude,
    double FromLongitude,
    double ToLatitude,
    double ToLongitude);


