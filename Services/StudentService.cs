public interface IStudentService
{
    Task<List<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(string id);
}
public class StudentService : IStudentService
{
    private readonly List<Student> _students =
    [
        new("S-001", "Abeba", 22, 3.8m),
        new("S-002", "Kidane", 21, 2.4m),
        new("S-003", "Dawit", 20, 3.1m)
    ];

    public Task<List<Student>> GetAllAsync()
        => Task.FromResult(_students);

    public Task<Student?> GetByIdAsync(string id)
        => Task.FromResult(
            _students.FirstOrDefault(s => s.Id == id));
}