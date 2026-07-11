using FluentValidation;

namespace TourPlanner.Application.UseCases.Tours.GetTourById;

public sealed class GetTourByIdRequestValidator : AbstractValidator<GetTourByIdRequest>
{
    public GetTourByIdRequestValidator()
    {
        RuleFor(x => x.TourId).NotEmpty().WithMessage("Tour ID is required.");
    }
}