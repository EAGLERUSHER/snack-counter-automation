using Microsoft.AspNetCore.Mvc;
using SnackCounter.Api.Models;
using SnackCounter.Api.Services;
using SnackCounter.Domain;

namespace SnackCounter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OfficesController : ControllerBase
{
    private readonly SnackCounterApplication _app;

    public OfficesController(SnackCounterApplication app) => _app = app;

    [HttpGet]
    public ActionResult<IEnumerable<OfficeInfoDto>> List()
    {
        var list = new List<OfficeInfoDto>();
        foreach (OfficeId office in Enum.GetValues<OfficeId>())
        {
            var spent = _app.Budget.GetSpentThisMonth(office);
            list.Add(new OfficeInfoDto(
                office,
                OfficeBudget.MonthlyBudget(office),
                OfficeBudget.GetCurrency(office),
                spent,
                _app.Budget.IsOverBudget(office)));
        }
        return list;
    }

    [HttpGet("{office}/active-refill")]
    public ActionResult<RefillDto?> ActiveRefill(OfficeId office)
    {
        var refill = _app.Refills.GetActiveByOffice(office);
        return refill is null ? null : Mapping.ToDto(refill);
    }
}