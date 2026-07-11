using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Common.Mapping;
using TourPlanner.Application.CommonDtos.TourLogs;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.TourLogs.GetTourLogsByTour;

public class GetTourLogsByTourIdUseCase (
    ITourRepository tours,
    ITourLogRepository tourlogs,
    ICurrentUserContext currentUser) : IUseCase<GetTourLogsByTourIdRequest, IReadOnlyList<TourLogResponseDto>> {
    
    public async Task<IReadOnlyList<TourLogResponseDto>> ExecuteAsync(GetTourLogsByTourIdRequest request, CancellationToken cancellationToken = default) {
        _ = await tours.GetByIdOrThrowAsync(request.TourLogId, currentUser.UserId, cancellationToken);

        var items = await tourlogs.GetByTourIdAsync(request.TourLogId, currentUser.UserId, cancellationToken);

        return items.Select(TourLogMapper.MapToResponse).ToList();
    }
    
}