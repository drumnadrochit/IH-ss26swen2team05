using Microsoft.EntityFrameworkCore;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Infrastructure.Persistence;

public sealed class UserRepository(TourPlannerDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        => dbContext.Users.FirstOrDefaultAsync(user => user.UserName == userName.Trim(), cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
        => dbContext.Users.AddAsync(user, cancellationToken).AsTask();
}

