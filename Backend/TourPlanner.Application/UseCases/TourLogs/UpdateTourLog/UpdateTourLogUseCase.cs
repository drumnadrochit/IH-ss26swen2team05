using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Common.Mapping;
using TourPlanner.Application.CommonDtos.TourLogs;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.TourLogs.UpdateTourLog;

public class UpdateTourLogUseCase (
    ITourRepository tours,
    ITourLogRepository tourLogs,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : IUseCase<UpdateTourLogRequest, TourLogResponseDto> {
    
    public async Task<TourLogResponseDto> ExecuteAsync(UpdateTourLogRequest request, CancellationToken cancellationToken = default) {
        var log = await tourLogs.GetByIdOrThrowAsync(request.TourLogId, currentUser.UserId, cancellationToken);
        
        log.Update(request.AccomplishedAt, request.Comment, request.Difficulty, request.TotalDistanceKm, request.TotalTimeMinutes, request.Rating);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tour = await tours.GetByIdOrThrowAsync(log.TourId, currentUser.UserId, cancellationToken);

        var allLogs = await tourLogs.GetByTourIdAsync(tour.Id, currentUser.UserId, cancellationToken);
        tour.RecalculateMetrics(allLogs);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return TourLogMapper.MapToResponse(log);
    }
}