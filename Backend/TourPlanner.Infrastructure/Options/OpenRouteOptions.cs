namespace TourPlanner.Infrastructure.Options;

public sealed class OpenRouteOptions
{
    public string BaseUrl { get; set; } = "https://api.openrouteservice.org";

    public string ApiKey { get; set; } = string.Empty;
}

