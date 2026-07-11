using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Mapping;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.Tours.GetAllTours;

public sealed class GetAllToursUseCase(
    ITourRepository tours,
    ICurrentUserContext currentUser) : IUseCase<GetAllToursRequest, IReadOnlyList<TourSummaryResponseDto>>
{
    public async Task<IReadOnlyList<TourSummaryResponseDto>> ExecuteAsync(GetAllToursRequest request, CancellationToken cancellationToken = default)
    {
        var items = await tours.GetByUserIdAsync(currentUser.UserId, cancellationToken);
        
        return items.Select(TourSummaryMapper.MapToResponse).ToArray();
    }
}