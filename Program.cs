using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Authentication & Authorization Services
builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training", null);

builder.Services.AddAuthorization();

var app = builder.Build();

// Exercise 1B Order
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler("/error");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Protected Endpoint
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}));
// .RequireAuthorization();

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
});
// .RequireAuthorization();
app.Run();