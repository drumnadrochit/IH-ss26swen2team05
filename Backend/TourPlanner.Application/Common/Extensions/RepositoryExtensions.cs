using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Contracts.Persistence;

public static class RepositoryExtensions
{
    /// <summary>
    /// Fetches a tour by ID and ensures it belongs to the current user, throwing a uniform exception if missing.
    /// </summary>
    public static async Task<Tour> GetByIdOrThrowAsync(
        this ITourRepository repository, 
        Guid tourId, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await repository.GetByIdAsync(tourId, userId, cancellationToken)
               ?? throw new TourPlannerNotFoundException($"Tour with ID '{tourId}' was not found or access is denied.");
    }

    /// <summary>
    /// Fetches a tour log by ID and ensures it belongs to the current user, throwing a uniform exception if missing.
    /// </summary>
    public static async Task<TourLog> GetByIdOrThrowAsync(
        this ITourLogRepository repository, 
        Guid tourLogId, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await repository.GetByIdAsync(tourLogId, userId, cancellationToken)
               ?? throw new TourPlannerNotFoundException($"Tour log with ID '{tourLogId}' was not found or access is denied.");
    }
}