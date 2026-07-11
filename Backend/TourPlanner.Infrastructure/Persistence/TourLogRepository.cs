using Microsoft.EntityFrameworkCore;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Domain;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Infrastructure.Persistence;

public sealed class TourLogRepository(TourPlannerDbContext dbContext) : ITourLogRepository
{
    public Task<TourLog?> GetByIdAsync(Guid tourLogId, Guid userId, CancellationToken cancellationToken = default)
        => dbContext.TourLogs
            .Include(log => log.Tour)
            .FirstOrDefaultAsync(log => log.Id == tourLogId && log.Tour.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<TourLog>> GetByTourIdAsync(Guid tourId, Guid userId, CancellationToken cancellationToken = default)
        => await dbContext.TourLogs
            .Include(log => log.Tour)
            .Where(log => log.TourId == tourId && log.Tour.UserId == userId)
            .OrderByDescending(log => log.AccomplishedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TourLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await dbContext.TourLogs
            .Include(log => log.Tour)
            .Where(log => log.Tour.UserId == userId)
            .OrderByDescending(log => log.AccomplishedAt)
            .ToListAsync(cancellationToken);
    
    public Task AddAsync(TourLog tourLog, CancellationToken cancellationToken = default)
        => dbContext.TourLogs.AddAsync(tourLog, cancellationToken).AsTask();

    public void Remove(TourLog tourLog) => dbContext.TourLogs.Remove(tourLog);
}

