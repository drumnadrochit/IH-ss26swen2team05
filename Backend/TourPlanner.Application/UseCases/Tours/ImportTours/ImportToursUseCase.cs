using System.Text;
using System.Text.Json;
using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Domain.Entities;
using TourPlanner.Domain.Metrics;

namespace TourPlanner.Application.UseCases.Tours.ImportTours;

public sealed class ImportToursUseCase(
    ITourRepository tours,
    ITourLogRepository tourLogs,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : IUseCase<ImportToursRequest, ImportToursResponse>
{
    public async Task<ImportToursResponse> ExecuteAsync(ImportToursRequest request, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(request.Content);
        var importedTours = JsonSerializer.Deserialize<List<TourDetailResponseDto>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("The import file does not contain valid tour data.");

        var importedTourCount = 0;
        var importedLogCount = 0;

        foreach (var importedTour in importedTours)
        {
            var tour = Tour.Create(
                currentUser.UserId, importedTour.Name, importedTour.Description, 
                importedTour.From, importedTour.To, importedTour.TransportType, 
                importedTour.DistanceKm, importedTour.EstimatedMinutes, importedTour.RouteInformation,
                routeGeoJson: importedTour.RouteGeoJson, fromLocation: null, toLocation: null);
            
            tour.UpdateImagePath(importedTour.ImagePath);

            await tours.AddAsync(tour, cancellationToken);
            importedTourCount++;

            var createdLogs = new List<TourLog>();
            foreach (var log in importedTour.Logs)
            {
                var importedLog = TourLog.Create(tour.Id, log.AccomplishedAt, log.Comment, log.Difficulty, log.TotalDistanceKm, log.TotalTimeMinutes, log.Rating);
                await tourLogs.AddAsync(importedLog, cancellationToken);
                createdLogs.Add(importedLog);
                importedLogCount++;
            }

            var metrics = TourMetricsCalculator.Calculate(createdLogs);
            tour.UpdateMetrics(metrics.Popularity, metrics.ChildFriendliness);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new ImportToursResponse(importedTourCount, importedLogCount);
    }
}