using Microsoft.EntityFrameworkCore;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Infrastructure.Persistence;

public sealed class TourRepository(TourPlannerDbContext dbContext) : ITourRepository
{
    public Task<Tour?> GetByIdAsync(Guid tourId, Guid userId, CancellationToken cancellationToken = default)
        => dbContext.Tours
            .Include(tour => tour.TourLogs)
            .FirstOrDefaultAsync(tour => tour.Id == tourId && tour.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Tour>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await dbContext.Tours
            .Where(tour => tour.UserId == userId)
            .OrderByDescending(tour => tour.UpdatedAt)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Tour tour, CancellationToken cancellationToken = default)
        => dbContext.Tours.AddAsync(tour, cancellationToken).AsTask();

    public void Remove(Tour tour) => dbContext.Tours.Remove(tour);
}

