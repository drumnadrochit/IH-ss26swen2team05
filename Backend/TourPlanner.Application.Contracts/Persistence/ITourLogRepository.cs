using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Contracts.Persistence;

public interface ITourLogRepository
{
    Task<TourLog?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TourLog>> GetByTourIdAsync(Guid tourId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TourLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task AddAsync(TourLog tourLog, CancellationToken cancellationToken = default);

    void Remove(TourLog tourLog);
}


