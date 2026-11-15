namespace SnackCounter.Domain;

public sealed class SnackCounterException : Exception
{
    public SnackCounterException(string message) : base(message) { }
    public string? ErrorCode { get; init; }
}

public static class ErrorCodes
{
    public const string Conflict = "CONFLICT";
    public const string BudgetExceeded = "BUDGET_EXCEEDED";
    public const string InvalidState = "INVALID_STATE";
    public const string NotAuthorized = "NOT_AUTHORIZED";
    public const string NoVolunteer = "NO_VOLUNTEER";
}