using System.Threading.Channels;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
// [Route("api/v2/transcripts")]
[Route("api/v{version:apiVersion}/transcripts")]
[ApiVersion("2.0")]
public class TranscriptsController(
    Channel<TranscriptRequest> channel,
    ITranscriptStatusStore statusStore) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscript(
        TranscriptRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await statusStore.GetReportIdForIdempotencyKeyAsync(idempotencyKey, ct);
            if (existing is not null)
            {
                var existingStatus = await statusStore.GetAsync(existing, ct);
                return Accepted(
                    Url.Action(nameof(GetStatus), new { id = existing }),
                    existingStatus);
            }
        }

        var reportId = Guid.NewGuid().ToString("N")[..12];
        var status = await statusStore.CreateAsync(reportId, request.StudentId, ct);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
            await statusStore.LinkIdempotencyKeyAsync(idempotencyKey, reportId, ct);

        await channel.Writer.WriteAsync(request.WithReportId(reportId), ct);

        Response.Headers.RetryAfter = "5";
        return Accepted(
            Url.Action(nameof(GetStatus), new { id = reportId }),
            status);
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(string id, CancellationToken ct)
    {
        var status = await statusStore.GetAsync(id, ct);
        return status is null
            ? NotFound(new ProblemDetails
            {
                Title = "Transcript not found",
                Detail = $"No transcript request with id '{id}'.",
                Status = StatusCodes.Status404NotFound
            })
            : Ok(status);
    }
}
// using Asp.Versioning;
// using MediatR;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.RateLimiting;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using TmsApi.Application.Transcripts.Queries;
// namespace TmsApi.Controllers.V2;

// [ApiController]
// [ApiVersion("2.0")]
// [Route("api/v{version:apiVersion}/transcripts")]
// public class TranscriptsController : ControllerBase
// {
//     private readonly IMediator _mediator;

//     public TranscriptsController(IMediator mediator)
//     {
//         _mediator = mediator;
//     }
//    [HttpPost]
// [EnableRateLimiting("transcripts")]
// public async Task<IActionResult> RequestTranscript(
//     [FromBody] object? request,
//     CancellationToken ct)
// {
//     Console.WriteLine($"START {DateTime.Now:HH:mm:ss.fff}");

//     await Task.Delay(10000, ct);

//     Console.WriteLine($"END   {DateTime.Now:HH:mm:ss.fff}");

//     return Ok();
// }
//     [HttpGet("search")]
// [EnableRateLimiting("search")]
//     public async Task<IActionResult> SearchCourses(
//         [FromQuery] string? term, CancellationToken ct)
//     {
//         var results = await _mediator.Send(new SearchCoursesQuery(term), ct);
//         return Ok(results);
//     }

//     // Minimal local query definition to satisfy build when the shared query type is missing.

// }