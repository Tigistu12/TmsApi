using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Asp.Versioning;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
using TmsApi.Application.Interfaces;
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
using Microsoft.Extensions.Caching.Hybrid;
using TmsApi.Infrastructure.Persistence.Context;
using TmsApi.Infrastructure.SeedData;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using TmsApi.Api.RateLimiting;
using System.Threading.Channels;
using TmsApi.Api.Hubs;
using TmsApi.Api.Notifications;
using TmsApi.Application.Notifications;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Infrastructure.Workers;

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

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
builder.Services.AddScoped<ITmsDbContext, TmsDbContext>();
builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();
builder.Services.AddSingleton<ITranscriptNotificationService, SignalRTranscriptNotificationService>();
builder.Services.AddHostedService<TranscriptWorker>();


builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(
    new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait
    }));

builder.Services.AddSignalR();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext,
    string>(httpContext =>
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);
        return tier switch
        {
            ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter
        (
        partitionKey: $"paid:{partitionKey}",
        factory: _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 200,
            TokensPerPeriod = 100,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            QueueLimit = 0,
            AutoReplenishment = true
        }),
            ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter
    (
    partitionKey: $"free:{partitionKey}",
    factory: _ => new TokenBucketRateLimiterOptions
    {
        TokenLimit = 30,
        TokensPerPeriod = 10,
        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
        QueueLimit = 0,
        AutoReplenishment = true
    }),
            _ => RateLimitPartition.GetTokenBucketLimiter(
        partitionKey: $"anon:{partitionKey}",
        factory: _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10,
            TokensPerPeriod = 5,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            QueueLimit = 0,
            AutoReplenishment = true
        })
        };
    });
    options.AddConcurrencyLimiter("transcripts", opt =>
    {
        opt.PermitLimit = 5; // 5 in-flight transcripts maximuM
        opt.QueueLimit = 20; // queue up to 20 more
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.AddTokenBucketLimiter("search", opt =>
    {
        opt.TokenLimit = 10;
        opt.TokensPerPeriod = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 2;
    });
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await context.HttpContext.Response.WriteAsync(
            "Too Many Requests",
            token);

        Console.WriteLine("Rate limiter rejected request.");
    };
    options.AddTokenBucketLimiter("anonymous", opt =>
{
    opt.TokenLimit = 10;
    opt.TokensPerPeriod = 5;
    opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
    opt.QueueLimit = 0;
    opt.AutoReplenishment = true;
});
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});


var app = builder.Build();
app.UseCors("AllowAngular");
app.UseMiddleware<V1DeprecationMiddleware>();
app.UseExceptionHandler();
app.MapControllers();

// Exercise 1B Order
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler("/error");

app.UseRouting();
app.UseRateLimiter();

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

// Map Hub endpoint
app.MapHub<TmsHub>("/hubs/tms");

app.Run();