using TourPlanner.Application.CommonDtos.TourLogs;

namespace TourPlanner.Application.CommonDtos.Tours;

public sealed record TourSearchResponseDto(IReadOnlyList<TourSummaryResponseDto> Tours, IReadOnlyList<TourLogResponseDto> TourLogs);

