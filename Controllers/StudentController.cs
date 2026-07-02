using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace TmsApi.Entities;
using TmsApi.Data;
[ApiController]
[Route("api/students")]

public class UpdateStudentRequest
{
    public required string Name { get; set; }
    public decimal Age { get; set; }    
    public int GPA { get; set; }
}
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly TmsDbContext _context;

    public StudentsController(IStudentService studentService, TmsDbContext context)
    {
        _studentService = studentService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _studentService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var student = await _studentService.GetByIdAsync(id);

        return student is not null
            ? Ok(student)
            : NotFound();
    }
    [HttpGet("small-page-response")]
    public async Task<IActionResult> GetStudents(
     int pageNumber = 1,
     int pageSize = 1,
     CancellationToken ct = default)
    {
        var students = await _context.Students
            .OrderBy(s => s.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(students);
    }

     [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentRequest request)
    {
        var student = await _context.Students//.AsNoTracking()
        . FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();

        student.Name = request.Name;
        student.GPA = (decimal)request.GPA;

        await _context.SaveChangesAsync();
        return Ok(student);
    }
}