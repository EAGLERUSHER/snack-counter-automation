namespace SnackCounter.Simulators;

using SnackCounter.Domain;
using SnackCounter.Ports;

public sealed class InMemoryHrReimbursementClient : IHrReimbursementClient
{
    private readonly Dictionary<string, HrClaim> _claims = new();
    private int _nextId;

    public string CreateClaim(string employeeId, decimal amount, Currency currency, string description, byte[] receiptImage)
    {
        var id = $"HR-{++_nextId}";
        _claims[id] = new HrClaim(id, employeeId, amount, currency, description, Approved: false);
        return id;
    }

    public void ApproveClaim(string claimId)
    {
        if (!_claims.TryGetValue(claimId, out var claim))
            throw new InvalidOperationException($"Claim {claimId} not found");
        _claims[claimId] = claim with { Approved = true };
    }

    public HrClaim? GetClaim(string claimId) =>
        _claims.TryGetValue(claimId, out var c) ? c : null;
}