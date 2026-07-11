using Microsoft.EntityFrameworkCore;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Infrastructure.Persistence;

public sealed class UserSessionRepository(TourPlannerDbContext dbContext) : IUserSessionRepository
{
    public Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => dbContext.UserSessions.FirstOrDefaultAsync(session => session.RefreshToken == refreshToken.Trim(), cancellationToken);

    public Task AddAsync(UserSession session, CancellationToken cancellationToken = default)
        => dbContext.UserSessions.AddAsync(session, cancellationToken).AsTask();

    public void Remove(UserSession session)
        => dbContext.UserSessions.Remove(session);
}

