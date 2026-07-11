namespace TourPlanner.Application.Abstractions.Mapping;

public interface IMappableToResponse<TDomain, TResponse> {
    static abstract TResponse MapToResponse(TDomain domain);
}