using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TourPlanner.Infrastructure.Persistence;

namespace TourPlanner.Infrastructure.Services;

public sealed class DatabaseCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<DatabaseCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(30); //set the time

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Database Cleanup Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Starting scheduled purge of expired user sessions...");

                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TourPlannerDbContext>();

                    var expiredSessions = dbContext.UserSessions
                        .Where(s => s.ExpiresAt <= DateTime.UtcNow);

                    dbContext.UserSessions.RemoveRange(expiredSessions);
                    var rowsDeleted = await dbContext.SaveChangesAsync(stoppingToken);

                    logger.LogInformation("Successfully purged {Count} expired sessions from the database.", rowsDeleted);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while cleaning up expired sessions.");
            }

            await Task.Delay(CleanupInterval, stoppingToken);
        }
    }
}