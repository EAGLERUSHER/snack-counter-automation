namespace SnackCounter.Services;

using SnackCounter.Domain;
using SnackCounter.Ports;

public sealed class RefillOrchestrator
{
    private readonly IRefillRepository _refills;
    private readonly IVolunteerRegistry _volunteers;
    private readonly VolunteerSelector _selector;
    private readonly BudgetService _budget;
    private readonly IChatNotifier _chat;
    private readonly IHrReimbursementClient _hr;
    private readonly IClock _clock;
    private readonly RefillOrchestratorOptions _options;
    private readonly object _commitLock = new();

    public RefillOrchestrator(
        IRefillRepository refills,
        IVolunteerRegistry volunteers,
        VolunteerSelector selector,
        BudgetService budget,
        IChatNotifier chat,
        IHrReimbursementClient hr,
        IClock clock,
        RefillOrchestratorOptions? options = null)
    {
        _refills = refills;
        _volunteers = volunteers;
        _selector = selector;
        _budget = budget;
        _chat = chat;
        _hr = hr;
        _clock = clock;
        _options = options ?? new RefillOrchestratorOptions();
    }

    public RefillRequest ReportEmpty(OfficeId officeId, string reporterEmployeeId)
    {
        var existing = _refills.GetActiveByOffice(officeId);
        if (existing is not null && existing.Status != RefillStatus.Reimbursed)
        {
            _chat.NotifyChannel(ChannelFor(officeId),
                $"Snack box still needs a refill (existing request {existing.Id}).");
            return existing;
        }

        var refill = new RefillRequest
        {
            Id = Guid.NewGuid().ToString("N"),
            OfficeId = officeId,
            ReportedByEmployeeId = reporterEmployeeId,
            Status = RefillStatus.ReportedEmpty,
            CreatedAt = _clock.UtcNow
        };
        _refills.Add(refill);
        _chat.NotifyChannel(ChannelFor(officeId), "Snack box is empty — coordinating a refill.");

        AssignVolunteer(refill.Id);
        return _refills.GetById(refill.Id)!;
    }

    public RefillRequest AssignVolunteer(string refillId)
    {
        var refill = RequireRefill(refillId);
        if (refill.Status is RefillStatus.Committed or RefillStatus.Purchased or RefillStatus.HrPending)
            throw new SnackCounterException("Cannot reassign while purchase is in progress.")
            { ErrorCode = ErrorCodes.InvalidState };

        var excluded = new HashSet<string>(StringComparer.Ordinal);
        if (refill.AssignedVolunteerId is not null)
            excluded.Add(refill.AssignedVolunteerId);

        var volunteer = _selector.SelectVolunteer(refill.OfficeId, excluded)
            ?? throw new SnackCounterException("No eligible volunteer found.")
            { ErrorCode = ErrorCodes.NoVolunteer };

        refill.AssignedVolunteerId = volunteer.Id;
        refill.Status = RefillStatus.Assigned;
        refill.AssignedAt = _clock.UtcNow;
        refill.AssignmentExpiresAt = _clock.UtcNow.Add(_options.AssignmentTtl);
        refill.CommittedByEmployeeId = null;
        refill.CommittedAt = null;

        _refills.Update(refill);
        _chat.NotifyEmployee(volunteer.Id,
            $"You are up for snack refill at {refill.OfficeId}. Please commit by {_options.AssignmentTtl.TotalHours}h.");
        _chat.NotifyChannel(ChannelFor(refill.OfficeId),
            $"{volunteer.Name} has been asked to refill the snack box.");

        return refill;
    }

    public RefillRequest CommitToRefill(string refillId, string employeeId)
    {
        lock (_commitLock)
        {
            var refill = RequireRefill(refillId);

            if (refill.CommittedByEmployeeId is not null)
                throw new SnackCounterException("Someone already committed to this refill.")
                { ErrorCode = ErrorCodes.Conflict };

            if (refill.Status is not (RefillStatus.Assigned or RefillStatus.Reassigned))
                throw new SnackCounterException("Refill is not open for commitment.")
                { ErrorCode = ErrorCodes.InvalidState };

            if (refill.AssignedVolunteerId != employeeId)
                throw new SnackCounterException("Only the assigned volunteer can commit.")
                { ErrorCode = ErrorCodes.NotAuthorized };

            if (refill.AssignmentExpiresAt is not null && _clock.UtcNow > refill.AssignmentExpiresAt)
                throw new SnackCounterException("Assignment window expired; wait for reassignment.")
                { ErrorCode = ErrorCodes.InvalidState };

            var employee = _volunteers.GetById(employeeId)
                ?? throw new SnackCounterException("Unknown employee.");

            refill.Status = RefillStatus.Committed;
            refill.CommittedByEmployeeId = employeeId;
            refill.CommittedAt = _clock.UtcNow;
            refill.PurchaseDeadlineAt = _clock.UtcNow.Add(_options.PurchaseTtlAfterCommit);

            employee.OpenRefillCommitmentId = refillId;
            _volunteers.Update(employee);
            _refills.Update(refill);

            _chat.NotifyChannel(ChannelFor(refill.OfficeId),
                $"{employee.Name} will buy snacks for the office.");
            return refill;
        }
    }

    public RefillRequest RecordPurchase(string refillId, string employeeId, decimal amount, Currency currency, byte[] receiptImage)
    {
        var refill = RequireRefill(refillId);

        if (refill.Status != RefillStatus.Committed)
            throw new SnackCounterException("Must commit before recording purchase.")
            { ErrorCode = ErrorCodes.InvalidState };

        if (refill.CommittedByEmployeeId != employeeId)
            throw new SnackCounterException("Only the committed volunteer can record purchase.")
            { ErrorCode = ErrorCodes.NotAuthorized };

        var expectedCurrency = OfficeBudget.GetCurrency(refill.OfficeId);
        if (currency != expectedCurrency)
            throw new SnackCounterException($"Expected currency {expectedCurrency} for {refill.OfficeId}.");

        if (!_budget.CanSpend(refill.OfficeId, amount))
            throw new SnackCounterException("Monthly snack budget would be exceeded.")
            { ErrorCode = ErrorCodes.BudgetExceeded };

        var (min, max) = OfficeBudget.TypicalPurchaseRange(refill.OfficeId);
        if (amount < min || amount > max)
        {
            _chat.NotifyChannel(ChannelFor(refill.OfficeId),
                $"Note: purchase {amount} {currency} is outside typical range {min}-{max} (allowed with receipt).");
        }

        refill.PurchaseAmount = amount;
        refill.PurchaseCurrency = currency;
        refill.ReceiptImage = receiptImage;
        refill.Status = RefillStatus.Purchased;

        _budget.RecordSpend(refill.OfficeId, amount, currency, refillId);
        _refills.Update(refill);

        _chat.NotifyEmployee(employeeId, "Thanks — please submit the receipt to HR (system will auto-file).");
        return refill;
    }

    public RefillRequest SubmitToHr(string refillId)
    {
        var refill = RequireRefill(refillId);

        if (refill.Status != RefillStatus.Purchased)
            throw new SnackCounterException("Purchase must be recorded before HR submission.")
            { ErrorCode = ErrorCodes.InvalidState };

        if (refill.PurchaseAmount is null || refill.PurchaseCurrency is null || refill.ReceiptImage is null)
            throw new SnackCounterException("Missing purchase details.");

        var volunteerId = refill.CommittedByEmployeeId!;
        var description = $"Snack counter refill — {refill.OfficeId}";
        refill.HrClaimId = _hr.CreateClaim(
            volunteerId,
            refill.PurchaseAmount.Value,
            refill.PurchaseCurrency.Value,
            description,
            refill.ReceiptImage);
        refill.Status = RefillStatus.HrPending;
        _refills.Update(refill);

        _chat.NotifyEmployee(volunteerId, $"HR claim {refill.HrClaimId} submitted; approval usually takes 1–10 days.");
        return refill;
    }

    public RefillRequest AdvanceHrSimulation(string refillId)
    {
        var refill = RequireRefill(refillId);
        if (refill.Status != RefillStatus.HrPending || refill.HrClaimId is null)
            throw new SnackCounterException("Refill is not awaiting HR.")
            { ErrorCode = ErrorCodes.InvalidState };

        _hr.ApproveClaim(refill.HrClaimId);
        refill.Status = RefillStatus.Reimbursed;
        refill.ReimbursedAt = _clock.UtcNow;

        var employee = _volunteers.GetById(refill.CommittedByEmployeeId!);
        if (employee is not null)
        {
            employee.LastRefillAt = _clock.UtcNow;
            employee.RefillCountThisQuarter++;
            employee.OpenRefillCommitmentId = null;
            _volunteers.Update(employee);
        }

        _refills.Update(refill);
        _chat.NotifyChannel(ChannelFor(refill.OfficeId), "Snack refill reimbursed — thanks to our volunteer!");
        return refill;
    }

    public RefillRequest? ReassignIfStale(string refillId)
    {
        var refill = RequireRefill(refillId);

        var needsReassign = refill.Status switch
        {
            RefillStatus.Assigned or RefillStatus.Reassigned =>
                refill.AssignmentExpiresAt is not null && _clock.UtcNow > refill.AssignmentExpiresAt &&
                refill.CommittedByEmployeeId is null,
            RefillStatus.Committed =>
                refill.PurchaseDeadlineAt is not null && _clock.UtcNow > refill.PurchaseDeadlineAt,
            _ => false
        };

        if (!needsReassign)
            return null;

        if (refill.Status == RefillStatus.Committed)
        {
            var employee = _volunteers.GetById(refill.CommittedByEmployeeId!);
            if (employee is not null)
            {
                employee.OpenRefillCommitmentId = null;
                _volunteers.Update(employee);
            }
        }

        refill.Status = RefillStatus.Reassigned;
        _refills.Update(refill);
        _chat.NotifyChannel(ChannelFor(refill.OfficeId), "Volunteer timed out — picking someone else.");

        return AssignVolunteer(refillId);
    }

    private RefillRequest RequireRefill(string id) =>
        _refills.GetById(id) ?? throw new SnackCounterException($"Refill {id} not found.");

    private static string ChannelFor(OfficeId office) => $"#snacks-{office.ToString().ToLowerInvariant()}";
}