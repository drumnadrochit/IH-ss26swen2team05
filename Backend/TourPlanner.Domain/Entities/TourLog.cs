namespace TourPlanner.Domain.Entities;

public sealed class TourLog : EntityBase
{
    public Guid TourId { get; private set; }

    public Tour Tour { get; private set; } = null!;

    public DateTimeOffset AccomplishedAt { get; private set; }

    public string Comment { get; private set; } = string.Empty;

    public TourDifficulty Difficulty { get; private set; }

    public double TotalDistanceKm { get; private set; }

    public double TotalTimeMinutes { get; private set; }

    public int Rating { get; private set; }

    private TourLog()
    {
    }

    public static TourLog Create(
        Guid tourId,
        DateTimeOffset accomplishedAt,
        string comment,
        TourDifficulty difficulty,
        double totalDistanceKm,
        double totalTimeMinutes,
        int rating)
    {
        Guard.NotEmpty(tourId, nameof(tourId));
        Guard.NotNullOrWhiteSpace(comment, nameof(comment));
        Guard.Range(rating, 1, 5, nameof(rating));

        return new TourLog
        {
            TourId = tourId,
            AccomplishedAt = accomplishedAt,
            Comment = comment.Trim(),
            Difficulty = difficulty,
            TotalDistanceKm = totalDistanceKm,
            TotalTimeMinutes = totalTimeMinutes,
            Rating = rating
        };
    }

    public void Update(
        DateTimeOffset accomplishedAt,
        string comment,
        TourDifficulty difficulty,
        double totalDistanceKm,
        double totalTimeMinutes,
        int rating)
    {
        Guard.NotNullOrWhiteSpace(comment, nameof(comment));
        Guard.Range(rating, 1, 5, nameof(rating));

        AccomplishedAt = accomplishedAt;
        Comment = comment.Trim();
        Difficulty = difficulty;
        TotalDistanceKm = totalDistanceKm;
        TotalTimeMinutes = totalTimeMinutes;
        Rating = rating;
        Touch();
    }

    public string BuildSearchDocument() => string.Join(
        ' ',
        Comment,
        Difficulty,
        TotalDistanceKm.ToString("0.##"),
        TotalTimeMinutes.ToString("0.##"),
        Rating,
        AccomplishedAt.ToString("O"));
}

