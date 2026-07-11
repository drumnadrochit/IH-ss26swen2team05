using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.UseCases.Tours.CreateTour;
using TourPlanner.Application.UseCases.Tours.DeleteTour;
using TourPlanner.Application.UseCases.Tours.GetAllTours;
using TourPlanner.Application.UseCases.Tours.GetRecommendedTours;
using TourPlanner.Application.UseCases.Tours.GetTourById;
using TourPlanner.Application.UseCases.Tours.GetTourImage;
using TourPlanner.Application.UseCases.Tours.GetTourInsights;
using TourPlanner.Application.UseCases.Tours.UpdateTour;
using TourPlanner.Application.UseCases.Tours.UploadTourImage;

namespace TourPlanner.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tours")]
public sealed class ToursController(
    IUseCase<GetAllToursRequest, IReadOnlyList<TourSummaryResponseDto>> getAllUseCase,
    IUseCase<GetTourByIdRequest, TourDetailResponseDto> getByIdUseCase,
    IUseCase<CreateTourRequest, TourSummaryResponseDto> createUseCase,
    IUseCase<UpdateTourRequest, TourSummaryResponseDto> updateUseCase,
    IUseCase<DeleteTourRequest> deleteUseCase,
    IUseCase<GetRecommendedToursRequest, IReadOnlyList<TourSummaryResponseDto>> getRecommendationsUseCase,
    IUseCase<GetTourInsightsRequest, TourInsightResponseDto> getInsightsUseCase,
    IUseCase<UploadTourImageRequest, UploadTourImageResponseDto> uploadImageUseCase,
    IUseCase<GetTourImageRequest, GetTourImageResponse> getImageUseCase
    ) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TourSummaryResponseDto>>> GetAll(CancellationToken ct)
        => Ok(await getAllUseCase.ExecuteAsync(new GetAllToursRequest(), ct));

    [HttpGet("{tourId:guid}")]
    public async Task<ActionResult<TourDetailResponseDto>> GetById(Guid tourId, CancellationToken ct)
        => Ok(await getByIdUseCase.ExecuteAsync(new GetTourByIdRequest(tourId), ct));

    [HttpPost]
    public async Task<ActionResult<TourSummaryResponseDto>> Create([FromBody] CreateTourRequest request, CancellationToken ct)
    {
        var created = await createUseCase.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { tourId = created.Id }, created);
    }

    [HttpPut("{tourId:guid}")]
    public async Task<ActionResult<TourSummaryResponseDto>> Update(Guid tourId, [FromBody] UpdateTourRequest request, CancellationToken ct)
        => Ok(await updateUseCase.ExecuteAsync(request with { TourId = tourId }, ct));

    [HttpDelete("{tourId:guid}")]
    public async Task<IActionResult> Delete(Guid tourId, CancellationToken ct)
    {
        await deleteUseCase.ExecuteAsync(new DeleteTourRequest(tourId), ct);
        return NoContent();
    }

    [HttpGet("recommendations")]
    public async Task<ActionResult<IReadOnlyList<TourSummaryResponseDto>>> GetRecommendations([FromQuery] int take = 5, CancellationToken ct = default)
        => Ok(await getRecommendationsUseCase.ExecuteAsync(new GetRecommendedToursRequest(take), ct));

    [HttpGet("{tourId:guid}/insights")]
    public async Task<ActionResult<TourInsightResponseDto>> GetInsights(Guid tourId, CancellationToken ct)
        => Ok(await getInsightsUseCase.ExecuteAsync(new GetTourInsightsRequest(tourId), ct));

    [HttpPost("{tourId:guid}/image")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<UploadTourImageResponseDto>> UploadImage(Guid tourId, [FromForm] IFormFile file, CancellationToken ct)
    {
        if (file.Length <= 0) {
            return BadRequest("The uploaded file is empty.");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, ct);

        var request = new UploadTourImageRequest(tourId, file.FileName, memoryStream.ToArray());
        return Ok(await uploadImageUseCase.ExecuteAsync(request, ct));
    }

    [HttpGet("{tourId:guid}/image")]
    public async Task<IActionResult> GetImage(Guid tourId, CancellationToken ct)
    {
        var result = await getImageUseCase.ExecuteAsync(new GetTourImageRequest(tourId), ct);
        return File(result.Content, result.ContentType);
    }
}