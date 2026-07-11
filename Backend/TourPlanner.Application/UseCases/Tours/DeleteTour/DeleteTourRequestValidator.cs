using FluentValidation;

namespace TourPlanner.Application.UseCases.Tours.DeleteTour;

public sealed class DeleteTourRequestValidator : AbstractValidator<DeleteTourRequest>
{
    public DeleteTourRequestValidator()
    {
        RuleFor(x => x.TourId)
            .NotEmpty().WithMessage("A valid tour identifier must be provided to delete a tour.");
    }
}