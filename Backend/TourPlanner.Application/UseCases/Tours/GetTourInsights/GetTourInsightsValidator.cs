using FluentValidation;

namespace TourPlanner.Application.UseCases.Tours.GetTourInsights;

public sealed class GetTourInsightsRequestValidator : AbstractValidator<GetTourInsightsRequest>
{
    public GetTourInsightsRequestValidator()
    {
        RuleFor(x => x.TourId).NotEmpty().WithMessage("Tour ID is required to fetch insights.");
    }
}