using TourPlanner.Domain.Entities;

namespace TourPlanner.Domain.Metrics;

public static class TourMetricsCalculator
{
    public static TourMetrics Calculate(IEnumerable<TourLog> logs)
    {
        var materialized = logs.ToArray();
        if (materialized.Length == 0)
        {
            return new TourMetrics(0, 0, 0, 0, 0, 100);
        }

        var logCount = materialized.Length;
        var averageDifficulty = materialized.Average(log => (int)log.Difficulty);
        var averageDistance = materialized.Average(log => log.TotalDistanceKm);
        var averageTime = materialized.Average(log => log.TotalTimeMinutes);
        var childFriendliness = Math.Clamp(100 - (averageDifficulty * 14.5) - (averageDistance * 2.2) - (averageTime * 0.35), 0, 100);

        return new TourMetrics(logCount, averageDifficulty, averageDistance, averageTime, logCount, childFriendliness);
    }
}

