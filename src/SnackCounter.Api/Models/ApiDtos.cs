using SnackCounter.Domain;

namespace SnackCounter.Api.Models;

public sealed record OfficeInfoDto(
    OfficeId Id,
    decimal MonthlyBudget,
    Currency Currency,
    decimal SpentThisMonth,
    bool OverBudget);

public sealed record EmployeeDto(
    string Id,
    string Name,
    OfficeId HomeOffice,
    WorkLocationStatus LocationStatus,
    OfficeId? VisitingOffice,
    DateTimeOffset? LastRefillAt,
    int RefillCountThisQuarter,
    string? OpenRefillCommitmentId);

public sealed record RefillDto(
    string Id,
    OfficeId OfficeId,
    RefillStatus Status,
    string ReportedByEmployeeId,
    string? AssignedVolunteerId,
    string? CommittedByEmployeeId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AssignmentExpiresAt,
    decimal? PurchaseAmount,
    Currency? PurchaseCurrency,
    string? HrClaimId);

public sealed record ChatMessageDto(string Target, string Text, DateTimeOffset At);

public sealed record MonthlyReportDto(
    int Year,
    int Month,
    IReadOnlyList<OfficeMonthlyLineDto> Offices,
    decimal TotalInReportingCurrency,
    Currency ReportingCurrency);

public sealed record OfficeMonthlyLineDto(
    OfficeId OfficeId,
    decimal Spent,
    decimal Budget,
    Currency Currency,
    bool OverBudget);

public sealed record ReportEmptyRequest(OfficeId OfficeId, string ReporterEmployeeId);
public sealed record CommitRequest(string EmployeeId);
public sealed record PurchaseRequest(string EmployeeId, decimal Amount);
public sealed record LoginRequest(string EmployeeId);
public sealed record LoginResponse(string Token, EmployeeDto Employee);
public sealed record ErrorResponse(string Message, string? Code);

public static class Mapping
{
    public static RefillDto ToDto(RefillRequest r) => new(
        r.Id, r.OfficeId, r.Status, r.ReportedByEmployeeId,
        r.AssignedVolunteerId, r.CommittedByEmployeeId, r.CreatedAt,
        r.AssignmentExpiresAt, r.PurchaseAmount, r.PurchaseCurrency, r.HrClaimId);

    public static EmployeeDto ToDto(Employee e) => new(
        e.Id, e.Name, e.HomeOffice, e.LocationStatus, e.VisitingOffice,
        e.LastRefillAt, e.RefillCountThisQuarter, e.OpenRefillCommitmentId);
}