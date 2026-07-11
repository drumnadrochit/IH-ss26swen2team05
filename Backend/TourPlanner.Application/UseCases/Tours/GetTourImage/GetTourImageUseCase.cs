using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Contracts.Files;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.Tours.GetTourImage;

public sealed class GetTourImageUseCase(
    ITourRepository tours,
    IFileStorage fileStorage,
    ICurrentUserContext currentUser) : IUseCase<GetTourImageRequest, GetTourImageResponse>
{
    public async Task<GetTourImageResponse> ExecuteAsync(GetTourImageRequest request, CancellationToken cancellationToken = default)
    {
        var tour = await tours.GetByIdOrThrowAsync(request.TourId, currentUser.UserId, cancellationToken);

        if (string.IsNullOrWhiteSpace(tour.ImagePath))
        {
            throw new TourPlannerNotFoundException("This tour has no image.");
        }

        var content = await fileStorage.ReadFileAsync(tour.ImagePath, cancellationToken)
            ?? throw new TourPlannerNotFoundException("The tour image could not be found.");

        return new GetTourImageResponse(content, GetContentType(tour.ImagePath));
    }

    private static string GetContentType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        _ => "application/octet-stream"
    };
}
