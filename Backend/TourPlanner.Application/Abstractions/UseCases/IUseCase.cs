namespace TourPlanner.Application.Abstractions.UseCases;

public interface IUseCase<in TRequest, TResponse> {
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IUseCase<in TRequest>
{
    Task ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}