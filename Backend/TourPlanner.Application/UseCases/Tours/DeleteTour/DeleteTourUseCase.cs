using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Contracts.Files;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.Tours.DeleteTour;

public class DeleteTourUseCase (
    ITourRepository tours,
    IFileStorage fileStorage,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : IUseCase<DeleteTourRequest> {
    
    public async Task ExecuteAsync(DeleteTourRequest request, CancellationToken cancellationToken = default) {
        var tour = await tours.GetByIdOrThrowAsync(request.TourId, currentUser.UserId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(tour.ImagePath))
        {
            await fileStorage.DeleteFileAsync(tour.ImagePath, cancellationToken);
        }

        tours.Remove(tour);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}