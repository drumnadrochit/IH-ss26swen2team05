using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Contracts.Security;

public interface ITokenService
{
    Task<TokenPair> GenerateTokenPairAsync(User user, CancellationToken cancellationToken = default);
}