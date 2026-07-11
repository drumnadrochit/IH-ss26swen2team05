using TourPlanner.Domain.Enums;
using TourPlanner.Domain.Metrics;
using TourPlanner.Domain.ValueObjects;

namespace TourPlanner.Domain.Entities;

public sealed class Tour : EntityBase
{
    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string From { get; private set; } = string.Empty;

    public string To { get; private set; } = string.Empty;

    public TransportType TransportType { get; private set; }

    public double DistanceKm { get; private set; }

    public double EstimatedMinutes { get; private set; }

    public string RouteInformation { get; private set; } = string.Empty;

    public string? ImagePath { get; private set; }

    public int Popularity { get; private set; }

    public double ChildFriendliness { get; private set; }

    public ICollection<TourLog> TourLogs { get; private set; } = new List<TourLog>();
    
    public string? RouteGeoJson { get; private set; }
    
    public Coordinates? FromLocation { get; private set; }
    public Coordinates? ToLocation { get; private set; }

    private Tour()
    {
    }

    public static Tour Create(
        Guid userId,
        string name,
        string description,
        string from,
        string to,
        TransportType transportType,
        double distanceKm,
        double estimatedMinutes,
        string routeInformation,
        string? routeGeoJson,
        Coordinates? fromLocation,
        Coordinates? toLocation)
    {
        Guard.NotEmpty(userId, nameof(userId));
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.NotNullOrWhiteSpace(description, nameof(description));
        Guard.NotNullOrWhiteSpace(from, nameof(from));
        Guard.NotNullOrWhiteSpace(to, nameof(to));
        Guard.NotNullOrWhiteSpace(routeInformation, nameof(routeInformation));

        return new Tour
        {
            UserId = userId,
            Name = name.Trim(),
            Description = description.Trim(),
            From = from.Trim(),
            To = to.Trim(),
            TransportType = transportType,
            DistanceKm = distanceKm,
            EstimatedMinutes = estimatedMinutes,
            RouteInformation = routeInformation.Trim(),
            RouteGeoJson = routeGeoJson,
            FromLocation = fromLocation,
            ToLocation = toLocation,
            Popularity = 0,
            ChildFriendliness = 100d
        };
    }

    public void Update(
        string name,
        string description,
        string from,
        string to,
        TransportType transportType,
        double distanceKm,
        double estimatedMinutes,
        string routeInformation,
        string? routeGeoJson,
        Coordinates? fromLocation,
        Coordinates? toLocation)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.NotNullOrWhiteSpace(description, nameof(description));
        Guard.NotNullOrWhiteSpace(from, nameof(from));
        Guard.NotNullOrWhiteSpace(to, nameof(to));
        Guard.NotNullOrWhiteSpace(routeInformation, nameof(routeInformation));

        Name = name.Trim();
        Description = description.Trim();
        From = from.Trim();
        To = to.Trim();
        TransportType = transportType;
        DistanceKm = distanceKm;
        EstimatedMinutes = estimatedMinutes;
        RouteInformation = routeInformation.Trim();
        
        RouteGeoJson = routeGeoJson;
        FromLocation = fromLocation;
        ToLocation = toLocation;
        Touch();
    }
    
    public void UpdateRoute(string geoJson, Coordinates fromLocation, Coordinates toLocation)
    {
        RouteGeoJson = geoJson;
        FromLocation = fromLocation;
        ToLocation = toLocation;
    }

    public void UpdateMetrics(int popularity, double childFriendliness)
    {
        Popularity = Math.Max(0, popularity);
        ChildFriendliness = Math.Clamp(childFriendliness, 0, 100);
        Touch();
    }

    public void RecalculateMetrics(IEnumerable<TourLog> logs) {
        
        Guard.AgainstNull(logs, nameof(logs));
        
        var metrics = TourMetricsCalculator.Calculate(logs);
        
        UpdateMetrics(metrics.Popularity, metrics.ChildFriendliness);
    }

    public void UpdateImagePath(string? imagePath)
    {
        ImagePath = string.IsNullOrWhiteSpace(imagePath) ? null : imagePath.Trim();
        Touch();
    }

    public string BuildSearchDocument() => string.Join(
        ' ',
        Name,
        Description,
        From,
        To,
        TransportType,
        DistanceKm.ToString("0.##"),
        EstimatedMinutes.ToString("0.##"),
        Popularity.ToString(),
        ChildFriendliness.ToString("0.##"),
        ImagePath ?? string.Empty,
        RouteInformation,
        FromLocation?.Latitude.ToString("0.####") ?? string.Empty,
        FromLocation?.Longitude.ToString("0.####") ?? string.Empty,
        ToLocation?.Latitude.ToString("0.####") ?? string.Empty,
        ToLocation?.Longitude.ToString("0.####") ?? string.Empty);
}

