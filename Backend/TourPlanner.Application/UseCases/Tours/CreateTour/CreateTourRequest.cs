namespace TourPlanner.Application.UseCases.Tours.CreateTour;

public sealed record CreateTourRequest(
    string Name,
    string Description,
    string From,
    string To,
    Domain.Enums.TransportType TransportType);