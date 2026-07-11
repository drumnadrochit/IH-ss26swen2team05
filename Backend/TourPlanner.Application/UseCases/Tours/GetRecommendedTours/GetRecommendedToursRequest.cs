namespace TourPlanner.Application.UseCases.Tours.GetRecommendedTours;

public sealed record GetRecommendedToursRequest(int Take = 5);