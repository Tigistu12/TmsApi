using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TmsApi.Application.Transcripts.Queries;
namespace TmsApi.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/transcripts")]
public class TranscriptsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TranscriptsController(IMediator mediator)
    {
        _mediator = mediator;
    }
   [HttpPost]
[EnableRateLimiting("transcripts")]
public async Task<IActionResult> RequestTranscript(
    [FromBody] object? request,
    CancellationToken ct)
{
    Console.WriteLine($"START {DateTime.Now:HH:mm:ss.fff}");

    await Task.Delay(10000, ct);

    Console.WriteLine($"END   {DateTime.Now:HH:mm:ss.fff}");

    return Ok();
}
    [HttpGet("search")]
[EnableRateLimiting("search")]
    public async Task<IActionResult> SearchCourses(
        [FromQuery] string? term, CancellationToken ct)
    {
        var results = await _mediator.Send(new SearchCoursesQuery(term), ct);
        return Ok(results);
    }

    // Minimal local query definition to satisfy build when the shared query type is missing.

}