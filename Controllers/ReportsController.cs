using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{
    
    [HttpGet("active-students")]
public async Task<IActionResult> GetActiveStudentsCount()
{
    var count = await context.Students
        .Where(s => s.IsActive && s.GPA >= 3.0m)
        .CountAsync();

    return Ok(new
    {
        Count = count
    });
}

[HttpGet("course-enrollments")]
public async Task<IActionResult> GetCourseEnrollments()
{
    var list = await context.Courses
        .Select(c => new
        {
            c.Title,
            EnrollmentCount = c.Enrollments.Count
        })
        .OrderByDescending(x => x.EnrollmentCount)
        .ToListAsync();

    return Ok(list);
}

[HttpGet("average-gpa")]
public async Task<IActionResult> GetAverageGpaPerCourse()
{
    var list = await context.Enrollments
        .GroupBy(e => e.Course.Title)
        .Select(g => new
        {
            Course = g.Key,
            AverageGPA = g.Average(e => e.Student.GPA)
        })
        .ToListAsync();

    return Ok(list);
}

[HttpGet("students-no-enrollment-a")]
public async Task<IActionResult> GetStudentsWithoutEnrollmentA()
{
    var list = await context.Students
        .Where(s => !s.Enrollments.Any())
        .Select(s => s.Name)
        .ToListAsync();

    return Ok(list);
}

[HttpGet("students-no-enrollment-b")]
public async Task<IActionResult> GetStudentsWithoutEnrollmentB()
{
    var list = await context.Students
        .LeftJoin(
            context.Enrollments,
            s => s.Id,
            e => e.StudentId,
            (s, e) => new { s, e })
        .Where(x => x.e == null)
        .Select(x => x.s.Name)
        .ToListAsync();

    return Ok(list);
}
}
