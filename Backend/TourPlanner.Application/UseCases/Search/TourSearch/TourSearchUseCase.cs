using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Mapping;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.Search.TourSearch;

public sealed class TourSearchUseCase(
    ITourRepository tours,
    ITourLogRepository tourLogs,
    ICurrentUserContext currentUser) : IUseCase<TourSearchRequest, TourSearchResponseDto>
{
    public async Task<TourSearchResponseDto> ExecuteAsync(TourSearchRequest request, CancellationToken cancellationToken = default) {
    
        var normalizedQuery = request.Query?.Trim() ?? string.Empty;

        // Sequential on purpose: both calls share the same scoped DbContext, and EF Core's
        // DbContext is not thread-safe, so running them concurrently via Task.WhenAll throws
        // "A second operation was started on this context instance before a previous
        // operation completed."
        var tourItems = await tours.GetByUserIdAsync(currentUser.UserId, cancellationToken);
        var logItems = await tourLogs.GetByUserIdAsync(currentUser.UserId, cancellationToken);
        
        var matchedTours = tourItems
            .Where(tour => Matches(tour.BuildSearchDocument(), normalizedQuery))
            .Select(TourSummaryMapper.MapToResponse)
            .ToArray();

        var matchedLogs = logItems
            .Where(log => Matches(log.BuildSearchDocument(), normalizedQuery))
            .Select(TourLogMapper.MapToResponse)
            .ToArray();

        return new TourSearchResponseDto(matchedTours, matchedLogs);
    }

    private static bool Matches(string document, string query)
        => string.IsNullOrWhiteSpace(query) || document.Contains(query, StringComparison.OrdinalIgnoreCase);

}


