using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Common.Mapping;
using TourPlanner.Application.CommonDtos.TourLogs;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.UseCases.TourLogs.CreateTourLog;

public class CreateTourLogUseCase (
        ITourRepository tours,
        ITourLogRepository tourLogs,
        ICurrentUserContext currentUser,
        IUnitOfWork unitOfWork) : IUseCase<CreateTourLogRequest, TourLogResponseDto>
    {
        public async Task<TourLogResponseDto> ExecuteAsync(CreateTourLogRequest request, CancellationToken cancellationToken = default) {
            var tour = await tours.GetByIdOrThrowAsync(request.TourId, currentUser.UserId, cancellationToken);
            
            var log = TourLog.Create(tour.Id, request.AccomplishedAt, request.Comment, request.Difficulty, request.TotalDistanceKm, request.TotalTimeMinutes, request.Rating);
            await tourLogs.AddAsync(log, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var allLogs = await tourLogs.GetByTourIdAsync(tour.Id, currentUser.UserId, cancellationToken);
            tour.RecalculateMetrics(allLogs);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return TourLogMapper.MapToResponse(log);
        }
}