namespace SnackCounter.Domain;

public sealed class Employee
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required OfficeId HomeOffice { get; init; }
    public WorkLocationStatus LocationStatus { get; set; } = WorkLocationStatus.InOffice;
    public OfficeId? VisitingOffice { get; set; }
    public DateTimeOffset? LastRefillAt { get; set; }
    public int RefillCountThisQuarter { get; set; }
    public string? OpenRefillCommitmentId { get; set; }
}