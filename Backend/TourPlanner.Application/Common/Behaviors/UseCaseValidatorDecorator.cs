using FluentValidation;
using TourPlanner.Application.Abstractions.UseCases;

namespace TourPlanner.Application.Common.Behaviors;

public sealed class UseCaseValidatorDecorator<TRequest, TResponse>(
    IUseCase<TRequest, TResponse> inner,
    IValidator<TRequest>? validator = null)
    : IUseCase<TRequest, TResponse> {
    
    
    public async Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default) {
        if (validator is not null) {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
        }
        
        return await inner.ExecuteAsync(request, cancellationToken);
    }
}