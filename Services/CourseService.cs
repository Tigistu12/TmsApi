using Microsoft.EntityFrameworkCore;
using TmsApi.Dtos;
using TmsApi.Data;
using TmsApi.Entities;
namespace TmsApi.Services;


public class CourseService(TmsDbContext context, ILogger<CourseService>logger) : ICourseService
{
public  Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
{
// TODO 1: Use context.Courses.AsNoTracking()
 return  context.Courses.AsNoTracking()
 .Where(c => c.Id == id)
 .Select(c => new CourseResponseDto(
    c.Id,
    c.Code,
    c.Title,
    c.MaxCapacity,
    c.Enrollments.Count))
 .FirstOrDefaultAsync(ct);

 throw new NotImplementedException();
}
public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
{
    var course = new Course
    {
        Code = request.Code,
        Title = request.Title,
        MaxCapacity = request.MaxCapacity
    };

 context.Courses.Add(course);

await context.SaveChangesAsync(ct);

logger.LogInformation(
    "Created Course {CourseId} ({Code})",
    course.Id,
    course.Code);

return (await GetByIdAsync(course.Id, ct))!;
throw new NotImplementedException();
}

public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
    {
        return context.Courses
        .AsNoTracking()
        .AnyAsync(c => c.Code == code, ct);
    }

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
PagedRequest request, CancellationToken ct)
    {
        IQueryable<Course> query = context.Courses.AsNoTracking();

         // apply search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c => 
            EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
            EF.Functions.ILike(c.Code, $"%{request.Search}%"));
        }

        // count before paging
        var totalCount = await query.CountAsync(ct);

        // apply sorting
        query = request.OrderBy switch
        {
            "Code" => request.Descending 
            ? query.OrderByDescending(c => c.Code)
            :query.OrderBy(c => c.Code),

            "MaxCapacity" => request.Descending
             ? query.OrderByDescending(c => c.MaxCapacity)
            : query.OrderBy(c => c.MaxCapacity),

            _ => request.Descending 
             ? query.OrderByDescending(c => c.Title)
            : query.OrderBy(c => c.Title)
        };

        // paging and projection
        var items = await query
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Select( c => new CourseResponseDto(
            c.Id,
            c.Code,
            c.Title,
            c.MaxCapacity,
            c.Enrollments.Count))
            .ToListAsync(ct);

            // 6 return paged response
            return new PagedResponse<CourseResponseDto> 
            { 
                Items = items,
                 TotalCount = totalCount, 
                 Page = request.Page, 
                 PageSize = request.PageSize 
                 };
            
      throw new NotImplementedException();
    }
}


// public interface ICourseService
// {
//     Task<List<Course>> GetAllAsync();
//     Task<Course?> GetByCodeAsync(string code);
// }
// public class CourseService : ICourseService
// {
//     private readonly List<Course> _courses =
//     [
//         new () {Code="CS-101", Title="C# Fundamentals",MaxCapacity=30 },
//         new (){
//             Code="WEB-201", Title="ASP.NET Core", MaxCapacity=4 },
//         new (){Code="DB-301", Title="SQL Server", MaxCapacity=3 }
//     ];

//     public Task<List<Course>> GetAllAsync()
//         => Task.FromResult(_courses);

//     public Task<Course?> GetByCodeAsync(string code)
//         => Task.FromResult(
//             _courses.FirstOrDefault(c => c.Code == code));
// }
