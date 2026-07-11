using FluentValidation;

namespace TourPlanner.Application.UseCases.Tours.CreateTour;

public sealed class CreateTourRequestValidator : AbstractValidator<CreateTourRequest>
{
    public CreateTourRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tour name is required.")
            .MaximumLength(100).WithMessage("Tour name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.From)
            .NotEmpty().WithMessage("Starting location is required.")
            .MaximumLength(100).WithMessage("Starting location cannot exceed 100 characters.");

        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Destination location is required.")
            .MaximumLength(100).WithMessage("Destination location cannot exceed 100 characters.");

        RuleFor(x => x.TransportType)
            .IsInEnum().WithMessage("Transport type must be a valid value.");
    }
}

