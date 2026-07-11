using FluentValidation;

namespace TourPlanner.Application.UseCases.TourLogs.CreateTourLog;

public sealed class CreateTourLogRequestValidator : AbstractValidator<CreateTourLogRequest>
{
    public CreateTourLogRequestValidator()
    {
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.TotalDistanceKm).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalTimeMinutes).GreaterThanOrEqualTo(0);
    }
}

