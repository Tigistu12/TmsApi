using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Asp.Versioning;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
using TmsApi.Application.Interfaces;
// using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;
using TmsApi.Api.Controllers;
using TmsApi.Api.Filters;
using Microsoft.EntityFrameworkCore;
using TmsApi.Api.Middlewares;
using FluentValidation;
using TmsApi.Application.Enrollments.Commands;
using MediatR;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Application.Behaviors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();



builder.Services.AddDbContext<TmsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));
// Authentication & Authorization Services
builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training", null);
      
builder.Services.AddControllers(options =>
{
options.Filters.Add<AuditLogFilter>();
});

builder.Services.AddOpenApi("v1", options =>
{
options.ShouldInclude = description =>
description.GroupName == "v1";
});
builder.Services.AddOpenApi("v2", options =>
{
options.ShouldInclude = description =>
description.GroupName == "v2";
});
builder.Services.AddApiVersioning(options =>
{
options.DefaultApiVersion = new ApiVersion(1, 0);
options.AssumeDefaultVersionWhenUnspecified = true;
options.ReportApiVersions = true;
options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
options.GroupNameFormat = "'v'VVV";
options.SubstituteApiVersionInUrl = true;
});



builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var app = builder.Build();

app.UseMiddleware<V1DeprecationMiddleware>();
app.UseExceptionHandler();
app.MapControllers();

// Exercise 1B Order
app.UseMiddleware<RequestLoggingMiddleware>();

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
 app.MapScalarApiReference(options =>
 {
     options.WithTitle("TMS API Reference")
     .WithTheme(ScalarTheme.DeepSpace)
     .WithDefaultHttpClient(ScalarTarget.CSharp,
      ScalarClient.HttpClient);

// tell scalar to pull both documents into its sidebar dropdown
   options.AddDocument("v1", "API Version 1.0")
   .AddDocument("v2", "API Version 2.0");
 });


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