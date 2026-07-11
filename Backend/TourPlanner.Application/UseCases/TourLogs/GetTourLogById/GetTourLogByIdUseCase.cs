using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Common.Mapping;
using TourPlanner.Application.CommonDtos.TourLogs;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.TourLogs.GetTourLogById;

public sealed class GetTourLogByIdUseCase (
    ITourLogRepository tourLogs,
    ICurrentUserContext currentUser) : IUseCase<GetTourLogByIdRequest, TourLogResponseDto> {
    
    public async Task<TourLogResponseDto> ExecuteAsync(GetTourLogByIdRequest request, CancellationToken cancellationToken = default) {
        var log = await tourLogs.GetByIdOrThrowAsync(request.TourLogId, currentUser.UserId, cancellationToken);

        return TourLogMapper.MapToResponse(log);
    }
}