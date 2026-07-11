using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Domain.Metrics;

namespace TourPlanner.Application.UseCases.Tours.GetTourInsights;

public sealed class GetTourInsightsUseCase(
    ITourRepository tours,
    ITourLogRepository tourLogs,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : IUseCase<GetTourInsightsRequest, TourInsightResponseDto>
{
    public async Task<TourInsightResponseDto> ExecuteAsync(GetTourInsightsRequest request, CancellationToken cancellationToken = default)
    {
        var tour = await tours.GetByIdOrThrowAsync(request.TourId, currentUser.UserId, cancellationToken);

        var logs = await tourLogs.GetByTourIdAsync(tour.Id, currentUser.UserId, cancellationToken);
        
        var metrics = TourMetricsCalculator.Calculate(logs);
        
        tour.UpdateMetrics(metrics.Popularity, metrics.ChildFriendliness);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TourInsightResponseDto(
            tour.Id, 
            metrics.LogCount, 
            metrics.AverageDifficulty, 
            metrics.AverageDistanceKm, 
            metrics.AverageTimeMinutes, 
            metrics.Popularity, 
            metrics.ChildFriendliness);
    }
}