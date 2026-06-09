using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Correlation ID
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        // 2. Add header BEFORE next
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        // 3. Start timer
        var stopwatch = Stopwatch.StartNew();

        // 4. Log request start
        _logger.LogInformation(
            "START {Method} {Path} [{CorrelationId}]",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        // 5. Call next middleware
        await _next(context);

        // 6. Stop timer
        stopwatch.Stop();

        // 7. Log request end
        _logger.LogInformation(
            "END Status:{StatusCode} Time:{Elapsed}ms [{CorrelationId}]",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}