using Microsoft.AspNetCore.Mvc;
using SnackCounter.Api.Models;
using SnackCounter.Api.Services;
using SnackCounter.Domain;

namespace SnackCounter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RefillsController : ControllerBase
{
    private readonly SnackCounterApplication _app;

    public RefillsController(SnackCounterApplication app) => _app = app;

    [HttpGet]
    public ActionResult<IEnumerable<RefillDto>> List() =>
        _app.Refills.ListAll().Select(Mapping.ToDto).ToList();

    [HttpGet("{id}")]
    public ActionResult<RefillDto> Get(string id)
    {
        var refill = _app.Refills.GetById(id);
        return refill is null ? NotFound() : Mapping.ToDto(refill);
    }

    [HttpPost("report-empty")]
    public ActionResult<RefillDto> ReportEmpty([FromBody] ReportEmptyRequest request)
    {
        try
        {
            var refill = _app.Orchestrator.ReportEmpty(request.OfficeId, request.ReporterEmployeeId);
            return Mapping.ToDto(refill);
        }
        catch (SnackCounterException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message, ex.ErrorCode));
        }
    }

    [HttpPost("{id}/commit")]
    public ActionResult<RefillDto> Commit(string id, [FromBody] CommitRequest request)
    {
        try
        {
            return Mapping.ToDto(_app.Orchestrator.CommitToRefill(id, request.EmployeeId));
        }
        catch (SnackCounterException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message, ex.ErrorCode));
        }
    }

    [HttpPost("{id}/purchase")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<RefillDto>> Purchase(
        string id,
        IFormFile? receipt,
        [FromForm] decimal amount,
        [FromForm] string? employeeId)
    {
        try
        {
            if (receipt is null || receipt.Length == 0)
                return BadRequest(new ErrorResponse("Receipt image or PDF is required.", "RECEIPT_REQUIRED"));

            var actor = HttpContext.Items["EmployeeId"] as string
                ?? throw new SnackCounterException("Not authenticated.");
            var who = string.IsNullOrWhiteSpace(employeeId) ? actor : employeeId;

            var refill = _app.Refills.GetById(id)
                ?? throw new SnackCounterException("Refill not found");
            var currency = OfficeBudget.GetCurrency(refill.OfficeId);

            await using var stream = new MemoryStream();
            await receipt.CopyToAsync(stream);
            var bytes = stream.ToArray();

            return Mapping.ToDto(_app.Orchestrator.RecordPurchase(
                id, who, amount, currency, bytes));
        }
        catch (SnackCounterException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message, ex.ErrorCode));
        }
    }

    [HttpPost("{id}/submit-hr")]
    public ActionResult<RefillDto> SubmitHr(string id)
    {
        try
        {
            return Mapping.ToDto(_app.Orchestrator.SubmitToHr(id));
        }
        catch (SnackCounterException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message, ex.ErrorCode));
        }
    }

    [HttpPost("{id}/approve-hr")]
    public ActionResult<RefillDto> ApproveHr(string id)
    {
        try
        {
            return Mapping.ToDto(_app.Orchestrator.AdvanceHrSimulation(id));
        }
        catch (SnackCounterException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message, ex.ErrorCode));
        }
    }

    [HttpPost("{id}/reassign-stale")]
    public ActionResult<RefillDto?> ReassignStale(string id)
    {
        try
        {
            var result = _app.Orchestrator.ReassignIfStale(id);
            return result is null ? NoContent() : Mapping.ToDto(result);
        }
        catch (SnackCounterException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message, ex.ErrorCode));
        }
    }
}