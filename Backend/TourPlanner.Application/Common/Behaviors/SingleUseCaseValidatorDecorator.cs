using FluentValidation;
using TourPlanner.Application.Abstractions.UseCases;

namespace TourPlanner.Application.Common.Behaviors;

public sealed class UseCaseValidatorDecorator<TRequest>(
    IUseCase<TRequest> inner,
    IValidator<TRequest>? validator = null)
    : IUseCase<TRequest>
{
    public async Task ExecuteAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        if (validator is not null)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
        }

        await inner.ExecuteAsync(request, cancellationToken);
    }
}