namespace SnackCounter.Domain;

public sealed record SpendEvent(
    string Id,
    OfficeId OfficeId,
    decimal Amount,
    Currency Currency,
    string RefillId,
    DateTimeOffset Timestamp);