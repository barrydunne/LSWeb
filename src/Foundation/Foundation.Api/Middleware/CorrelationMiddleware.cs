using Serilog.Context;

namespace Foundation.Api.Middleware;

/// <summary>
/// Ensures every request and response carries a correlation id so that related
/// log entries can be traced. An inbound correlation id is honoured when present;
/// otherwise a new one is generated.
/// </summary>
public class CorrelationMiddleware
{
    private const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next delegate in the request pipeline.</param>
    public CorrelationMiddleware(RequestDelegate next)
        => _next = next;

    /// <summary>
    /// Invokes the middleware for the current request.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that completes when the pipeline has finished processing the request.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
