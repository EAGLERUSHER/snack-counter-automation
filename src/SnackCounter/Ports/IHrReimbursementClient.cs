namespace SnackCounter.Ports;

using SnackCounter.Domain;

public interface IHrReimbursementClient
{
    string CreateClaim(string employeeId, decimal amount, Currency currency, string description, byte[] receiptImage);
    void ApproveClaim(string claimId);
    HrClaim? GetClaim(string claimId);
}

public sealed record HrClaim(
    string Id,
    string EmployeeId,
    decimal Amount,
    Currency Currency,
    string Description,
    bool Approved);