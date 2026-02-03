using System.Diagnostics;

namespace DirectPayGateway.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            if (elapsed > 1000)
            {
                _logger.LogWarning(
                    "Slow request: {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                    requestMethod, requestPath, statusCode, elapsed);
            }
            else
            {
                _logger.LogInformation(
                    "{Method} {Path} responded {StatusCode} in {Elapsed}ms",
                    requestMethod, requestPath, statusCode, elapsed);
            }
        }
    }
}
