using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Common.Mapping;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.Tours.GetTourById;

public sealed class GetTourByIdUseCase(
    ITourRepository tours,
    ITourLogRepository tourLogs,
    ICurrentUserContext currentUser) : IUseCase<GetTourByIdRequest, TourDetailResponseDto>
{
    public async Task<TourDetailResponseDto> ExecuteAsync(GetTourByIdRequest request, CancellationToken cancellationToken = default)
    {
        var tour = await tours.GetByIdOrThrowAsync(request.TourId, currentUser.UserId, cancellationToken);

        var logs = await tourLogs.GetByTourIdAsync(tour.Id, currentUser.UserId, cancellationToken);

        var mappedLogs = logs.Select(TourLogMapper.MapToResponse).ToArray();

        return new TourDetailResponseDto(
            tour.Id,
            tour.Name,
            tour.Description,
            tour.From,
            tour.To,
            tour.TransportType,
            tour.DistanceKm,
            tour.EstimatedMinutes,
            tour.RouteInformation,
            tour.Popularity,
            tour.ChildFriendliness,
            tour.ImagePath,
            mappedLogs,
            tour.RouteGeoJson);
    }
}