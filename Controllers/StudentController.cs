using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Dtos;
using TmsApi.Entities;
using TmsApi.Services;

namespace TmsApi.Controllers;
[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

public class StudentsController(IStudentService studentService, LinkGenerator linkGenerator) : ControllerBase
{

[HttpGet]
[ProducesResponseType(typeof(PagedResponse<StudentResponseDto>), StatusCodes.Status200OK)]
[EndpointSummary("List students with pagination")]

public async Task<IActionResult> GetStudents([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await studentService.GetStudentsAsync(request, ct);

        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    [ProducesResponseType(typeof(StudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a student by ID")]

    public async Task<IActionResult> GetStudentById( int id, CancellationToken ct)
    {
        var student = await studentService.GetByIdAsync(id, ct);

        if (student is null)
        return NotFound();

        var self = linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetStudentById),
            new{id = student.Id})!;

            var links = new List<LinkDto>
            {
                new(self, "self", "GET"),
                new(self, "update", "PUT"),
                new(self, "delete", "DELETE"),
            };

            var detail = new StudentDetailDto
            {
                Id = student.Id,
                RegistrationNumber = student.RegistrationNumber,
                Name = student.Name,
                Age = student.Age,
                GPA = student.GPA,
                Links = links
            };

            return Ok(detail);
    }

[HttpPost]
[ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[EndpointSummary("Create a new student")]

public async Task<IActionResult> CreateStudent(CreateStudentRequest request, CancellationToken ct)
    {
        if(await studentService.RegistrationExistsAsync(request.RegistrationNumber, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Student registration number already exists",
                Detail = $" A student with registration number '{request.RegistrationNumber}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await studentService.CreateAsync(request, ct);

        return CreatedAtAction(
            nameof(GetStudentById),
            new{id = result.Id},
            result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a student")]
    [EndpointDescription("Updates a student with a unique registration number. Returns404 if the student does not exist.")]

    public async Task<IActionResult> UpdateStudent(int id, UpdateStudentRequest request, CancellationToken ct)
    {
        var student = await studentService.UpdateAsync(id, request, ct);

        if (student is null)
        return NotFound();

        return Ok(student);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a student")]
    [EndpointDescription("Deletes a student with a unique registration number. Returns404 if the student does not exist.")]

    public async Task<IActionResult> DeleteStudent( int id, CancellationToken ct)
    {
        var deleted = await studentService.DeleteAsync(id, ct);

        if(!deleted)
        return NotFound();

        return NoContent();
    }
}