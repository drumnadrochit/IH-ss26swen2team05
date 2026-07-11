using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TourPlanner.Application.Abstractions.UseCases;

namespace TourPlanner.Application.Common.Behaviors;

public sealed class UseCaseLoggingDecorator<TRequest>(
    IUseCase<TRequest> inner,
    ILogger<UseCaseLoggingDecorator<TRequest>> logger) 
    : IUseCase<TRequest>
{
    public async Task ExecuteAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling Use Case: {RequestName}", requestName);
        
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await inner.ExecuteAsync(request, cancellationToken);
            
            stopwatch.Stop();
            logger.LogInformation("Handled Use Case: {RequestName} successfully in {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Use Case {RequestName} failed after {ElapsedMs}ms with message: {Message}", 
                requestName, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}