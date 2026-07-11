using FluentValidation;

namespace TourPlanner.Application.UseCases.Tours.UpdateTour;

public sealed class UpdateTourRequestValidator : AbstractValidator<UpdateTourRequest>
{
    public UpdateTourRequestValidator()
    {
        RuleFor(x => x.TourId)
            .NotEmpty().WithMessage("Tour identifier is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tour name is required.")
            .MaximumLength(100).WithMessage("Tour name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.From)
            .NotEmpty().WithMessage("Starting location is required.");

        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Destination location is required.");
        
        RuleFor(x => x.TransportType)
            .IsInEnum().WithMessage("Transport type must be a valid value.");
    }
}

