using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.CommonDtos.TourLogs;
using TourPlanner.Application.UseCases.TourLogs.CreateTourLog;
using TourPlanner.Application.UseCases.TourLogs.GetTourLogsByTour;

namespace TourPlanner.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tours/{tourId:guid}/logs")]
public sealed class TourLogsController(
    IUseCase<GetTourLogsByTourIdRequest, IReadOnlyList<TourLogResponseDto>> getByTourIdUseCase,
    IUseCase<CreateTourLogRequest, TourLogResponseDto> createUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TourLogResponseDto>>> GetByTourId(Guid tourId, CancellationToken ct)
        => Ok(await getByTourIdUseCase.ExecuteAsync(new GetTourLogsByTourIdRequest(tourId), ct));

    [HttpPost]
    public async Task<ActionResult<TourLogResponseDto>> Create(Guid tourId, [FromBody] CreateTourLogRequest request, CancellationToken ct)
    {
        // Stamps the route parameter straight into the immutable record, overriding anything sent in the body
        var result = await createUseCase.ExecuteAsync(request with { TourId = tourId }, ct);
        
        return CreatedAtAction(nameof(GetByTourId), new { tourId }, result);
    }
}