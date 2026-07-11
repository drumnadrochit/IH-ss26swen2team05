using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.Application.Abstractions;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.UseCases.Tours.ExportTours;
using TourPlanner.Application.UseCases.Tours.ImportTours;

namespace TourPlanner.API.Controllers;

[ApiController]
[Authorize]
[Route("api/import-export")]
public sealed class ImportExportController(
    IUseCase<ImportToursRequest, ImportToursResponse> importUseCase,
    IUseCase<EmptyRequest, ExportToursRequest> exportUseCase
    ) : ControllerBase
{
    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var response = await exportUseCase.ExecuteAsync(new EmptyRequest(), cancellationToken);
        return File(response.Content, response.ContentType, response.FileName);
    }

    [HttpPost("import")]
    [RequestSizeLimit(25_000_000)]
    public async Task<ActionResult<ImportToursResponse>> Import([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
        {
            return BadRequest("The uploaded file is empty.");
        }

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);

        var request = new ImportToursRequest(file.FileName, stream.ToArray());
        return Ok(await importUseCase.ExecuteAsync(request, cancellationToken));
    }
}

