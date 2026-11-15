namespace SnackCounter.Infrastructure;

using SnackCounter.Domain;
using SnackCounter.Ports;

public sealed class InMemoryRefillRepository : IRefillRepository
{
    private readonly Dictionary<string, RefillRequest> _byId = new();
    private readonly object _lock = new();

    public void Add(RefillRequest refill)
    {
        lock (_lock)
        {
            _byId[refill.Id] = refill;
        }
    }

    public RefillRequest? GetById(string id)
    {
        lock (_lock)
        {
            return _byId.TryGetValue(id, out var r) ? Clone(r) : null;
        }
    }

    public RefillRequest? GetActiveByOffice(OfficeId officeId)
    {
        lock (_lock)
        {
            var r = _byId.Values
                .Where(x => x.OfficeId == officeId && x.IsActive && x.Status != RefillStatus.Reimbursed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();
            return r is null ? null : Clone(r);
        }
    }

    public void Update(RefillRequest refill)
    {
        lock (_lock)
        {
            if (!_byId.ContainsKey(refill.Id))
                throw new InvalidOperationException($"Refill {refill.Id} not found");
            _byId[refill.Id] = Clone(refill);
        }
    }

    public IReadOnlyList<RefillRequest> ListAll()
    {
        lock (_lock)
        {
            return _byId.Values.Select(Clone).OrderByDescending(r => r.CreatedAt).ToList();
        }
    }

    private static RefillRequest Clone(RefillRequest r) => new()
    {
        Id = r.Id,
        OfficeId = r.OfficeId,
        ReportedByEmployeeId = r.ReportedByEmployeeId,
        Status = r.Status,
        AssignedVolunteerId = r.AssignedVolunteerId,
        CommittedByEmployeeId = r.CommittedByEmployeeId,
        CreatedAt = r.CreatedAt,
        AssignedAt = r.AssignedAt,
        AssignmentExpiresAt = r.AssignmentExpiresAt,
        CommittedAt = r.CommittedAt,
        PurchaseDeadlineAt = r.PurchaseDeadlineAt,
        PurchaseAmount = r.PurchaseAmount,
        PurchaseCurrency = r.PurchaseCurrency,
        ReceiptImage = r.ReceiptImage,
        HrClaimId = r.HrClaimId,
        ReimbursedAt = r.ReimbursedAt
    };
}