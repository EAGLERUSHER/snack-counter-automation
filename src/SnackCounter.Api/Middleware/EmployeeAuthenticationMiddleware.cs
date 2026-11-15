using System.Text.Json;
using SnackCounter.Api.Services;

namespace SnackCounter.Api.Middleware;

public sealed class EmployeeAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public EmployeeAuthenticationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AuthSessionStore sessions)
    {
        var path = context.Request.Path.Value ?? "";

        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (IsAnonymousApi(path, context.Request.Method))
        {
            await _next(context);
            return;
        }

        var header = context.Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await WriteUnauthorized(context);
            return;
        }

        var token = header["Bearer ".Length..].Trim();
        if (!sessions.TryGetEmployee(token, out var employeeId))
        {
            await WriteUnauthorized(context);
            return;
        }

        context.Items["EmployeeId"] = employeeId;
        await _next(context);
    }

    private static bool IsAnonymousApi(string path, string method) =>
        path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase) ||
        (path.Equals("/api/employees", StringComparison.OrdinalIgnoreCase) && method == "GET");

    private static async Task WriteUnauthorized(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Sign in required." }));
    }
}