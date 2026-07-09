// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using TmsApi.Data;
// using System.Linq;
// using Tms.Api.Services;
// [ApiController]
// [Route("api/courses")]
// public class CoursesController(
//     ICourseService courseService,
//     TmsDbContext context)
//     : ControllerBase
// {
//        private readonly TmsDbContext _context = context;
//     [HttpGet]
//     public async Task<IActionResult> GetAll()
//     {
//         return Ok(await courseService.GetAllAsync());
//     }

//     [HttpGet("{code}")]
//     public async Task<IActionResult> GetByCode(string code)
//     {
//         var course = await courseService.GetByCodeAsync(code);

//         return course is not null
//             ? Ok(course)
//             : NotFound();
//     }
//     [HttpGet("top-5-courses")]
//     public async Task<IActionResult> GetTop5Courses(
//         CancellationToken ct = default)
//     {
//         var topCourses = await _context.Enrollments
//              .GroupBy(e => e.CourseId)
//              .Select(g => new
//              {
//                CourseId = g.Key,
//                 EnrollmentCount = g.Count()  
//              })
//              .OrderByDescending(x => x.EnrollmentCount)
//              .Take(5)
//              .ToListAsync(ct);

//         return Ok(topCourses);
//     }
// }


using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;
using TmsApi.Dtos;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        return course is not null ? Ok(course): NotFound();

        throw new NotImplementedException();

    }
    [HttpPost]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        if(await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A Course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await courseService.CreateAsync(request, ct);

        return CreatedAtAction(
         nameof(GetCourseById),
         new{id =  result.Id},
         result);
         throw new NotImplementedException();
    }

    [HttpGet]
public async Task<IActionResult> GetCourses(
[FromQuery] PagedRequest request, CancellationToken ct)
{
var result = await courseService.GetCoursesAsync(request, ct);
return Ok(result);
}
}