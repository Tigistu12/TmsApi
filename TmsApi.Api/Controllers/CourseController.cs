using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence.Context;

namespace TmsApi.Api.Controllers;

[Authorize(Roles = "Instructor, Admin")]
[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService courseService,
    LinkGenerator linkGenerator,
    IAuthorizationService authorizationService,
    TmsDbContext context) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if (course is null)
            return NotFound();

        var self = linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetCourseById),
            new { id = course.Id })!;

        var enrollments = linkGenerator.GetPathByName(
            HttpContext,
            "ListCourseEnrollments",
            new { courseId = course.Id })!;

        var links = new List<LinkDto>
        {
            new (self, "self", "GET"),
            new (self, "update", "PUT"),
            new (self, "delete", "DELETE"),
            new (enrollments, "enrollments", "GET")
        };

        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(new(enrollments, "enroll", "POST"));
        }

        var detail = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };
        return Ok(detail);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription("Creates a course with a unique code. Returns 409 if the course code already exists.")]
 public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
{
    if (await courseService.CodeExistsAsync(request.Code, ct))
    {
        return Conflict(new ProblemDetails
        {
            Title = "Course code already exists",
            Detail = $"A Course with code '{request.Code}' is already registered.",
            Status = StatusCodes.Status409Conflict
        });
    }

// Automatically set InstructorId to current user ID if not provided in payload
    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var modifiedRequest = request with 
    { 
        InstructorId = request.InstructorId ?? currentUserId 
    };

    var result = await courseService.CreateAsync(modifiedRequest, ct);

    return CreatedAtAction(
        nameof(GetCourseById),
        new { id = result.Id },
        result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EndpointSummary("Update a course")]
    [EndpointDescription("Updates a course with a unique code. Requires lead instructor course ownership or Admin role.")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        UpdateCourseRequest request,
        CancellationToken ct)
    {
        var courseEntity = await context.Courses.FindAsync([id], cancellationToken: ct);
        if (courseEntity is null)
            return NotFound();

        // Enforce Resource-Based Ownership Authorization Policy
        var authResult = await authorizationService.AuthorizeAsync(User, courseEntity, "CanEditCourse");
        if (!authResult.Succeeded)
        {
            return Forbid(); // 403 Forbidden when caller doesn't own the resource
        }

        var updatedCourse = await courseService.UpdateAsync(id, request, ct);
        return Ok(updatedCourse);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a course")]
    [EndpointDescription("Deletes a course with a unique code. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken ct)
    {
        var deleted = await courseService.DeleteAsync(id, ct);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}