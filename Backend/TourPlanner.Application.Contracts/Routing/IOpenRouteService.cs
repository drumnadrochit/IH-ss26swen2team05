using TourPlanner.Domain.Enums;

namespace TourPlanner.Application.Contracts.Routing;

public interface IOpenRouteService
{
    Task<RoutePlan> BuildRouteAsync(string from, string to, TransportType transportType, CancellationToken cancellationToken = default);
}


