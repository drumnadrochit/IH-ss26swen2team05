using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TourPlanner.Application.Contracts.Routing;
using TourPlanner.Domain.Enums;
using TourPlanner.Infrastructure.Options;

namespace TourPlanner.Infrastructure.Services;

public sealed class OpenRouteServiceClient(HttpClient httpClient, IOptions<OpenRouteOptions> options, ILogger<OpenRouteServiceClient> logger) : IOpenRouteService
{
    public async Task<RoutePlan> BuildRouteAsync(string from, string to, TransportType transportType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            return CreateFallbackRoute(from, to, transportType);
        }

        double[]? fromCoordinates = null;
        double[]? toCoordinates = null;

        try
        {
            fromCoordinates = await GeocodeAsync(from, cancellationToken);
            toCoordinates = await GeocodeAsync(to, cancellationToken);
            var profile = MapProfile(transportType);
            var requestUri = $"/v2/directions/{profile}/geojson";

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            // ORS expects the raw key with no auth scheme prefix. HttpHeaders.Add() validates
            // Authorization as a structured "scheme credential" pair and throws FormatException
            // for a bare value like this — TryAddWithoutValidation skips that client-side parsing.
            request.Headers.TryAddWithoutValidation("Authorization", options.Value.ApiKey);
            request.Content = JsonContent.Create(new
            {
                coordinates = new[] { fromCoordinates, toCoordinates }
            });

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("ORS directions request failed with {StatusCode} for profile {Profile}: {Body}", response.StatusCode, profile, body);
                return CreateFallbackRoute(from, to, transportType, fromCoordinates, toCoordinates);
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation("ORS directions succeeded for {From} -> {To} ({Profile})", from, to, profile);
            using var document = JsonDocument.Parse(json);
            var summary = document.RootElement.GetProperty("features")[0].GetProperty("properties").GetProperty("summary");
            var distanceKm = summary.GetProperty("distance").GetDouble() / 1000.0;
            var durationMinutes = summary.GetProperty("duration").GetDouble() / 60.0;
            
            var routeInformation = $"Route via {transportType} from {from} to {to}. Distance: {distanceKm:0.#} km. Est. Time: {durationMinutes:0} mins.";
            
            return new RoutePlan(
                distanceKm,
                durationMinutes,
                routeInformation,
                json, 
                fromCoordinates[1],
                fromCoordinates[0],
                toCoordinates[1],
                toCoordinates[0]);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "ORS routing failed for {From} -> {To}, falling back to estimate", from, to);
            return CreateFallbackRoute(from, to, transportType, fromCoordinates, toCoordinates);
        }
    }

    private async Task<double[]> GeocodeAsync(string location, CancellationToken cancellationToken)
    {
        var requestUri = $"/geocode/search?text={Uri.EscapeDataString(location)}&size=1";
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.TryAddWithoutValidation("Authorization", options.Value.ApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var feature = document.RootElement.GetProperty("features")[0];
        var coordinates = feature.GetProperty("geometry").GetProperty("coordinates");
        return new[] { coordinates[0].GetDouble(), coordinates[1].GetDouble() };
    }

    // ORS only has driving-car, driving-hgv, cycling-*, foot-*, and wheelchair profiles —
    // there's no public-transport/train/bus profile, so those fall back to driving-car.
    private static string MapProfile(TransportType transportType)
        => transportType switch
        {
            TransportType.Walking => "foot-walking",
            TransportType.Hiking => "foot-walking",
            TransportType.Bicycling => "cycling-regular",
            TransportType.Car => "driving-car",
            TransportType.PublicTransport => "driving-car",
            TransportType.Train => "driving-car",
            TransportType.Bus => "driving-car",
            _ => "driving-car"
        };

    private static RoutePlan CreateFallbackRoute(string from, string to, TransportType transportType, double[]? fallbackFrom = null, double[]? fallbackTo = null)
    {
        var distance = Math.Max(1, ((from.Length + to.Length) * 0.75) + ((int)transportType * 1.5));
        var speed = transportType switch
        {
            TransportType.Walking => 5d,
            TransportType.Hiking => 4d,
            TransportType.Bicycling => 15d,
            TransportType.Car => 55d,
            TransportType.PublicTransport => 30d,
            TransportType.Train => 80d,
            TransportType.Bus => 35d,
            _ => 25d
        };

        var minutes = (distance / speed) * 60d;
        var routeInformation = JsonSerializer.Serialize(new
        {
            type = "FeatureCollection",
            fallback = true,
            from,
            to,
            transportType = transportType.ToString(),
            distanceKm = Math.Round(distance, 2),
            estimatedMinutes = Math.Round(minutes, 2)
        });

        var fromLat = fallbackFrom != null && fallbackFrom.Length > 1 ? fallbackFrom[1] : 0d;
        var fromLng = fallbackFrom != null && fallbackFrom.Length > 0 ? fallbackFrom[0] : 0d;
        var toLat = fallbackTo != null && fallbackTo.Length > 1 ? fallbackTo[1] : 0d;
        var toLng = fallbackTo != null && fallbackTo.Length > 0 ? fallbackTo[0] : 0d;

        return new RoutePlan(
            Math.Round(distance, 2),
            Math.Round(minutes, 2),
            routeInformation,
            routeInformation,
            fromLat,
            fromLng,
            toLat,
            toLng);
    }
}