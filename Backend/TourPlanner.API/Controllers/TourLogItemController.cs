using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.CommonDtos.TourLogs;
using TourPlanner.Application.UseCases.TourLogs.DeleteTourLog;
using TourPlanner.Application.UseCases.TourLogs.GetTourLogById;
using TourPlanner.Application.UseCases.TourLogs.UpdateTourLog;

namespace TourPlanner.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tour-logs")]
public sealed class TourLogItemController(
    IUseCase<GetTourLogByIdRequest, TourLogResponseDto> getByIdUseCase,
    IUseCase<UpdateTourLogRequest, TourLogResponseDto> updateUseCase,
    IUseCase<DeleteTourLogRequest> deleteUseCase) : ControllerBase
{
    [HttpGet("{tourLogId:guid}")]
    public async Task<ActionResult<TourLogResponseDto>> GetById(Guid tourLogId, CancellationToken ct)
        => Ok(await getByIdUseCase.ExecuteAsync(new GetTourLogByIdRequest(tourLogId), ct));

    [HttpPut("{tourLogId:guid}")]
    public async Task<ActionResult<TourLogResponseDto>> Update(Guid tourLogId, [FromBody] UpdateTourLogRequest request, CancellationToken ct)
        => Ok(await updateUseCase.ExecuteAsync(request with { TourLogId = tourLogId }, ct));

    [HttpDelete("{tourLogId:guid}")]
    public async Task<IActionResult> Delete(Guid tourLogId, CancellationToken ct)
    {
        await deleteUseCase.ExecuteAsync(new DeleteTourLogRequest(tourLogId), ct);
        return NoContent();
    }
}