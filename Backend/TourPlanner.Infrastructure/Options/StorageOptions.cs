namespace TourPlanner.Infrastructure.Options;

public sealed class StorageOptions
{
    public string BasePath { get; set; } = Path.Combine(AppContext.BaseDirectory, "storage");
}

