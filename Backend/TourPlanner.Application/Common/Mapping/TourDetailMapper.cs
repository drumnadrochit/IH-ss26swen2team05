// using TourPlanner.Application.Abstractions.Mapping;
// using TourPlanner.Application.Dtos.Tours;
// using TourPlanner.Domain.Entities;
//
// namespace TourPlanner.Application.Common.Mapping;
//
// public class TourDetailMapper : IMappableToResponse<Tour, TourDetailResponseDto> {
//     public static TourDetailResponseDto MapToResponse(Tour tour) => new (
//         tour.Id,
//         tour.Name,
//         tour.Description,
//         tour.From,
//         tour.To,
//         tour.TransportType,
//         tour.DistanceKm,
//         tour.EstimatedMinutes,
//         tour.RouteInformation,
//         tour.Popularity,
//         tour.ChildFriendliness,
//         tour.ImagePath,
//         mappedLogs);
// }