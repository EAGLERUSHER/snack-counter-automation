using Microsoft.AspNetCore.Mvc;
using SnackCounter.Api.Models;
using SnackCounter.Api.Services;
using SnackCounter.Domain;

namespace SnackCounter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly SnackCounterApplication _app;

    public EmployeesController(SnackCounterApplication app) => _app = app;

    [HttpGet]
    public ActionResult<IEnumerable<EmployeeDto>> List()
    {
        var all = new List<EmployeeDto>();
        foreach (OfficeId office in Enum.GetValues<OfficeId>())
        {
            foreach (var e in _app.Volunteers.GetVolunteersForOffice(office))
            {
                if (all.All(x => x.Id != e.Id))
                    all.Add(Mapping.ToDto(e));
            }
        }
        return all.OrderBy(e => e.HomeOffice).ThenBy(e => e.Name).ToList();
    }
}