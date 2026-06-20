using Microsoft.AspNetCore.Mvc;
using TmsApi.Entities;
[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await studentService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var student = await studentService.GetByIdAsync(id);

        return student is not null
            ? Ok(student)
            : NotFound();
    }
}