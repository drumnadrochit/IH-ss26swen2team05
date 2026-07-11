using System.Text.Json;
using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.CommonDtos.TourLogs;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.Tours.ExportTours;

public class ExportToursUseCase (
    ITourRepository tours,
    ITourLogRepository tourLogs,
    ICurrentUserContext currentUser
    ) : IUseCase<EmptyRequest, ExportToursRequest> {
    public async Task<ExportToursRequest> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default) {
        var payload = new List<TourDetailResponseDto>();
        var userTours = await tours.GetByUserIdAsync(currentUser.UserId, cancellationToken);

        foreach (var tour in userTours)
        {
            var logs = await tourLogs.GetByTourIdAsync(tour.Id, currentUser.UserId, cancellationToken);
            payload.Add(new TourDetailResponseDto(
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
                logs.Select(log => new TourLogResponseDto(log.Id, log.TourId, log.AccomplishedAt, log.Comment, log.Difficulty, log.TotalDistanceKm, log.TotalTimeMinutes, log.Rating)).ToArray(),
                tour.RouteGeoJson));
        }

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var fileName = $"tour-planner-export-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.json";
        
        return new ExportToursRequest(fileName, "application/json", bytes);
    }
}