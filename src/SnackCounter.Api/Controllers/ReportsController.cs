using Microsoft.AspNetCore.Mvc;
using SnackCounter.Api.Models;
using SnackCounter.Api.Services;
using SnackCounter.Domain;

namespace SnackCounter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly SnackCounterApplication _app;

    public ReportsController(SnackCounterApplication app) => _app = app;

    [HttpGet("monthly")]
    public ActionResult<MonthlyReportDto> Monthly(
        [FromQuery] int? year,
        [FromQuery] int? month)
    {
        var now = _app.Clock.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;
        var report = _app.Reports.BuildReport(y, m);
        return new MonthlyReportDto(
            report.Year,
            report.Month,
            report.Offices.Select(o => new OfficeMonthlyLineDto(
                o.OfficeId, o.Spent, o.Budget, o.Currency, o.OverBudget)).ToList(),
            report.TotalInReportingCurrency,
            report.ReportingCurrency);
    }
}