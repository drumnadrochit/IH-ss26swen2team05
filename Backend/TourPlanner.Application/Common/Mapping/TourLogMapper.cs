using TourPlanner.Application.Abstractions.Mapping;
using TourPlanner.Application.CommonDtos.TourLogs;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Common.Mapping;

public sealed class TourLogMapper : IMappableToResponse<TourLog, TourLogResponseDto> {
    public static TourLogResponseDto MapToResponse(TourLog log) => new(
        log.Id,
        log.TourId,
        log.AccomplishedAt,
        log.Comment,
        log.Difficulty,
        log.TotalDistanceKm,
        log.TotalTimeMinutes,
        log.Rating
        );
}