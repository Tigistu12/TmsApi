using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace TmsApi.Controllers.V2;
using TmsApi.Application.DTOs;
using TmsApi.Infrastructure.Services;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Infrastructure.Persistence.Context;
using TmsApi.Application.Interfaces;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController( ICourseService courseService, ICachedCourseService cachedCourseService) : ControllerBase
{
   [HttpGet]
   [EnableRateLimiting("anonymous")]
public async Task<IActionResult> GetCourses(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
{
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 50);

    var courses = await cachedCourseService.GetAllCoursesAsync(ct);

    var totalCount = courses.Count;

    var rows = courses
        .OrderBy(c => c.Title)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    return Ok(new
    {
        data = rows,
        meta = new
        {
            totalCount,
            page,
            pageSize,
            totalPages,
            hasNext = page < totalPages,
            hasPrevious = page > 1
        }
    });
}
    [HttpPut("{id:int}")]
public async Task<IActionResult> UpdateCourse(
    int id,
    UpdateCourseRequest request,
    CancellationToken ct)
{
    var course = await courseService.UpdateAsync(id, request, ct);

    if (course is null)
    {
        return NotFound();
    }

    // Remove cached courses after a successful update
    await cachedCourseService.InvalidateCourseCacheAsync(ct);

    return Ok(course);
}
}