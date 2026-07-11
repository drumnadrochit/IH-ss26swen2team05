using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.Contracts.Files;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.Tours.UploadTourImage;

public sealed class UploadTourImageUseCase(
    ITourRepository tours,
    IFileStorage fileStorage,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : IUseCase<UploadTourImageRequest, UploadTourImageResponseDto>
{
    public async Task<UploadTourImageResponseDto> ExecuteAsync(UploadTourImageRequest request, CancellationToken cancellationToken = default) {
        var tour = await tours.GetByIdOrThrowAsync(request.TourId, currentUser.UserId, cancellationToken);

        var safeName = Path.GetFileName(request.FileName);
        var storagePath = Path.Combine("tour-images", currentUser.UserId.ToString("N"), tour.Id.ToString("N"), safeName)
            .Replace('\\', '/');

        var savedPath = await fileStorage.SaveFileAsync(storagePath, request.FileContent, cancellationToken);
        
        tour.UpdateImagePath(savedPath);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadTourImageResponseDto(savedPath);
    }
}