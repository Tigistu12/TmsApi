using TmsApi.Entities;
public interface ICourseService
{
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByCodeAsync(string code);
}
public class CourseService : ICourseService
{
    private readonly List<Course> _courses =
    [
        new ( "CS-101","C# Fundamentals",30 ),
        new (
            "WEB-201", "ASP.NET Core", 4 ),
        new ("DB-301","SQL Server", 3 )
    ];

    public Task<List<Course>> GetAllAsync()
        => Task.FromResult(_courses);

    public Task<Course?> GetByCodeAsync(string code)
        => Task.FromResult(
            _courses.FirstOrDefault(c => c.Code == code));
}
