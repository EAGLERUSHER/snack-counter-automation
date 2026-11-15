namespace SnackCounter.Domain;

public sealed class RefillRequest
{
    public required string Id { get; init; }
    public required OfficeId OfficeId { get; init; }
    public required string ReportedByEmployeeId { get; init; }
    public RefillStatus Status { get; set; }
    public string? AssignedVolunteerId { get; set; }
    public string? CommittedByEmployeeId { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? AssignedAt { get; set; }
    public DateTimeOffset? AssignmentExpiresAt { get; set; }
    public DateTimeOffset? CommittedAt { get; set; }
    public DateTimeOffset? PurchaseDeadlineAt { get; set; }
    public decimal? PurchaseAmount { get; set; }
    public Currency? PurchaseCurrency { get; set; }
    public byte[]? ReceiptImage { get; set; }
    public string? HrClaimId { get; set; }
    public DateTimeOffset? ReimbursedAt { get; set; }

    public bool IsActive => Status is not (RefillStatus.Reimbursed);
}