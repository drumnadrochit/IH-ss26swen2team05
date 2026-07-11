using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.TourLogs.DeleteTourLog;

public class DeleteTourLogUseCase (
    ITourRepository tours,
    ITourLogRepository tourLogs,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : IUseCase<DeleteTourLogRequest> {
    
    public async Task ExecuteAsync(DeleteTourLogRequest request, CancellationToken cancellationToken = default) {
        var log = await tourLogs.GetByIdOrThrowAsync(request.TourLogId, currentUser.UserId, cancellationToken);

        var tourId = log.TourId;
        tourLogs.Remove(log);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        var tour = await tours.GetByIdOrThrowAsync(tourId, currentUser.UserId, cancellationToken);

        var remainingLogs = await tourLogs.GetByTourIdAsync(tourId, currentUser.UserId, cancellationToken);
        tour.RecalculateMetrics(remainingLogs);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}