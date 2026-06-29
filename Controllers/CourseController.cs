using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using System.Linq;
[ApiController]
[Route("api/courses")]
public class CoursesController(
    ICourseService courseService,
    TmsDbContext context)
    : ControllerBase
{
       private readonly TmsDbContext _context = context;
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await courseService.GetAllAsync());
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var course = await courseService.GetByCodeAsync(code);

        return course is not null
            ? Ok(course)
            : NotFound();
    }
    [HttpGet("top-5-courses")]
    public async Task<IActionResult> GetTop5Courses(
        CancellationToken ct = default)
    {
        var topCourses = await _context.Enrollments
             .GroupBy(e => e.CourseId)
             .Select(g => new
             {
               CourseId = g.Key,
                EnrollmentCount = g.Count()  
             })
             .OrderByDescending(x => x.EnrollmentCount)
             .Take(5)
             .ToListAsync(ct);

        return Ok(topCourses);
    }
}