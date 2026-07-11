using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Mapping;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Application.Contracts.Routing;
using TourPlanner.Domain.Entities;
using TourPlanner.Domain.ValueObjects;

namespace TourPlanner.Application.UseCases.Tours.CreateTour;

public class CreateTourUseCase (
    ITourRepository tours,
    IOpenRouteService routeService,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : IUseCase<CreateTourRequest, TourSummaryResponseDto> {
    
    public async Task<TourSummaryResponseDto> ExecuteAsync(CreateTourRequest request, CancellationToken cancellationToken = default) {
        var route = await routeService.BuildRouteAsync(request.From, request.To, request.TransportType,
            cancellationToken);
        
        var fromLocation = new Coordinates(route.FromLatitude, route.FromLongitude);
        var toLocation = new Coordinates(route.ToLatitude, route.ToLongitude);

        var tour = Tour.Create(
            currentUser.UserId,
            request.Name,
            request.Description,
            request.From,
            request.To,
            request.TransportType,
            route.DistanceKm,
            route.EstimatedMinutes,
            route.RouteInformation,
            route.GeoJson,
            fromLocation,
            toLocation);

        await tours.AddAsync(tour, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return TourSummaryMapper.MapToResponse(tour);
    }
}