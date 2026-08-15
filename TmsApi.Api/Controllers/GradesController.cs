using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Grades.Commands;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/grades")]
public class GradesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PostGrade([FromBody] PostGradeCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }
}