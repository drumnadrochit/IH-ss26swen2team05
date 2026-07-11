using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Common.Extensions;

public static class TourRepositoryExtensions {
    public static async Task<Tour> EnsureExistsAsync(this ITourRepository repository, Guid tourId, Guid userId, CancellationToken cancellationToken)
    => await repository.GetByIdAsync(tourId, userId, cancellationToken) ?? throw new TourPlannerNotFoundException("Tour was not found");
}