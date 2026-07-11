using TourPlanner.Domain.Enums;

namespace TourPlanner.Application.UseCases.Tours.UpdateTour;

public sealed record UpdateTourRequest(
    Guid TourId,
    string Name,
    string Description,
    string From,
    string To,
    TransportType TransportType);

