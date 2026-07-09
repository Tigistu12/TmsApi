using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Services;
using TmsApi.Filters;
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<TmsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
.LogTo(Console.WriteLine, LogLevel.Information) // Log SQL to output window
.EnableSensitiveDataLogging());
// Authentication & Authorization Services
builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training", null);
      
builder.Services.AddControllers(options =>
{
options.Filters.Add<AuditLogFilter>();
});
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddSingleton<IStudentService, StudentService>();
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(); // Required before MapOpenApi() will work
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var app = builder.Build();
app.MapControllers();

// Exercise 1B Order
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseExceptionHandler("/error");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePages();
// Environment-specific configuration
if (app.Environment.IsDevelopment())
{
    // OpenAPI document
    app.MapOpenApi();

    // Scalar UI
 app.MapScalarApiReference();
 using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
await DataSeeder.SeedAsync(context);
}
else
{
    // Production error handling
    app.UseExceptionHandler();
}
// Protected Endpoint
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();


app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

app.MapGet("/payment-options", (IOptions<PaymentOptions> options) =>
    {
        return Results.Ok(options.Value);
    });

app.MapGet("/api/assessments/results1", (HttpContext context) =>
{
    return Results.Ok(new
    {
        User = context.User.Identity?.Name,
        IsAuthenticated = context.User.Identity?.IsAuthenticated,
        CourseCode = "CS-101",
        StudentId = "S-001",
        LetterGrade = "A"
    });
}).RequireAuthorization();

// app.MapGet("/api/error", () =>
// {
// throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
// });

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate(); // Applies any pending migrations; keeps migration history intact
    if (!context.Students.Any())
    {
        var students = new List<Student>
{
    new() {RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith",Age= 25, GPA = 3.8m, IsActive = true },
    new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones",Age= 22, GPA = 2.9m, IsActive = true },
    new() { RegistrationNumber ="TMS-2026-0003", Name = "Charlie Brown",Age = 30, GPA = 3.4m, IsActive = false },
    new() { RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince",Age = 25, GPA = 3.9m, IsActive = true },
    new() { RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright",Age = 21, GPA = 2.5m, IsActive = true }
};
        context.Students.AddRange(students);
        var courses = new List<Course>
{
        new() { Code = "CS-101", Title = "Introduction to Computer Science", MaxCapacity = 30 },
        new() { Code = "CS-201", Title = "Data Structures and Algorithms", MaxCapacity = 25 },
        new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity =40 }
};
        context.Courses.AddRange(courses);
        context.SaveChanges();
        var enrollments = new List<Enrollment>
{
new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
};  
        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }
}
app.Run();


