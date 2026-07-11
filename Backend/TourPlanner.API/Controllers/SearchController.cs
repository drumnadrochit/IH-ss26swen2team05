using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.Application.Abstractions;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.UseCases.Search.TourSearch;

namespace TourPlanner.API.Controllers;

[ApiController]
[Route("api/search")]
public sealed class SearchController(IUseCase<TourSearchRequest, TourSearchResponseDto> searchUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TourSearchResponseDto>> Search([FromQuery] string q, CancellationToken cancellationToken)
        => Ok(await searchUseCase.ExecuteAsync(new TourSearchRequest(q), cancellationToken));
}

