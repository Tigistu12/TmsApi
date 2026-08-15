using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Api.Hubs;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Hubs;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;
namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
#pragma warning disable CS9113 // Parameter is unread.
public class EnrollmentsController(IMediator mediator, IHubContext<TmsHub, ITmsHubClient> hubContext, Application.Interfaces.ITmsDbContext context) : ControllerBase
#pragma warning restore CS9113 // Parameter is unread.
{
    // Fetches actual records from database via MediatR
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var enrollments = await mediator.Send(new GetAllEnrollmentQuery(), ct);
        return Ok(enrollments);
    }

    [HttpPost]
    public async Task<IActionResult> Enroll(
        EnrollStudentCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.Match<IActionResult>(
            onSuccess: created => CreatedAtAction(
                nameof(GetSchedule),
                new { studentId = created.StudentId },
                created),
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" => StatusCodes.Status404NotFound,
                    "course_full" or "already_enrolled" => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest
                };
                return Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type: $"http://tms.local/errors/{error.Code}");
            });
    }

    [HttpGet("{studentId}/schedule")]
    public async Task<IActionResult> GetSchedule(
        int studentId, CancellationToken ct)
    {
        var schedule = await mediator.Send(
            new GetStudentScheduleQuery(studentId), ct);
        return Ok(schedule);
    }
    // POST /api/v2/enrollments/1/approve
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(
        int id,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new ApproveEnrollmentCommand(id),
            ct);

        // Tell every connected Angular client
        await hubContext.Clients.All.ReceiveEnrollmentStatusUpdated(
        id,
        "Approved");

        return Ok(result);


    }
   [HttpPut("{id}/reject")]
public async Task<IActionResult> Reject(
    int id,
    CancellationToken ct)
{
    var result = await mediator.Send(
        new RejectEnrollmentCommand(id),
        ct);

    if (result is null)
    {
        return NotFound(new
        {
            message = $"Enrollment {id} not found."
        });
    }

    await hubContext.Clients.All.ReceiveEnrollmentStatusUpdated(
        id,
        "Rejected");

    return Ok(result);
}
}