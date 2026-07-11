using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Mapping;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.Tours.GetRecommendedTours;

public sealed class GetRecommendedToursUseCase(
    ITourRepository tours,
    ICurrentUserContext currentUser) : IUseCase<GetRecommendedToursRequest, IReadOnlyList<TourSummaryResponseDto>>
{
    public async Task<IReadOnlyList<TourSummaryResponseDto>> ExecuteAsync(GetRecommendedToursRequest request, CancellationToken cancellationToken = default)
    {
        var items = await tours.GetByUserIdAsync(currentUser.UserId, cancellationToken);
        
        var limit = Math.Max(1, request.Take);

        return items
            .OrderByDescending(tour => tour.ChildFriendliness)
            .ThenByDescending(tour => tour.Popularity)
            .Take(limit)
            .Select(TourSummaryMapper.MapToResponse)
            .ToArray();
    }
}