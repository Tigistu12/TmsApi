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
        new () {Code="CS-101", Title="C# Fundamentals",Capacity=30 },
        new (){
            Code="WEB-201", Title="ASP.NET Core", Capacity=4 },
        new (){Code="DB-301", Title="SQL Server", Capacity=3 }
    ];

    public Task<List<Course>> GetAllAsync()
        => Task.FromResult(_courses);

    public Task<Course?> GetByCodeAsync(string code)
        => Task.FromResult(
            _courses.FirstOrDefault(c => c.Code == code));
}
