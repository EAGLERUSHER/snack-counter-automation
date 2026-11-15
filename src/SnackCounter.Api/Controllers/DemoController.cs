using Microsoft.AspNetCore.Mvc;
using SnackCounter.Api.Services;

namespace SnackCounter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly SnackCounterApplication _app;

    public DemoController(SnackCounterApplication app) => _app = app;

    [HttpPost("advance-hours")]
    public ActionResult<DateTimeOffset> AdvanceHours([FromQuery] double hours)
    {
        _app.Clock.Advance(TimeSpan.FromHours(hours));
        return _app.Clock.UtcNow;
    }
}