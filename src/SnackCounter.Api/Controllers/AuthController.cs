using Microsoft.AspNetCore.Mvc;
using SnackCounter.Api.Models;
using SnackCounter.Api.Services;

namespace SnackCounter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SnackCounterApplication _app;
    private readonly AuthSessionStore _sessions;

    public AuthController(SnackCounterApplication app, AuthSessionStore sessions)
    {
        _app = app;
        _sessions = sessions;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var employee = _app.Volunteers.GetById(request.EmployeeId);
        if (employee is null)
            return Unauthorized(new ErrorResponse("Unknown employee ID.", "AUTH_FAILED"));

        var token = _sessions.CreateSession(employee.Id);
        return new LoginResponse(token, Mapping.ToDto(employee));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var header = Request.Headers.Authorization.ToString();
        if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            _sessions.Revoke(header["Bearer ".Length..].Trim());
        return NoContent();
    }

    [HttpGet("me")]
    public ActionResult<EmployeeDto> Me()
    {
        var id = HttpContext.Items["EmployeeId"] as string;
        if (id is null) return Unauthorized();
        var employee = _app.Volunteers.GetById(id);
        return employee is null ? Unauthorized() : Mapping.ToDto(employee);
    }
}