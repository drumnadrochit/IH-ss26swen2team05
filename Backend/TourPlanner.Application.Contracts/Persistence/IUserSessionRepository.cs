using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Contracts.Persistence;

public interface IUserSessionRepository
{
    Task AddAsync(UserSession session, CancellationToken cancellationToken = default);

    Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    void Remove(UserSession session);
}


