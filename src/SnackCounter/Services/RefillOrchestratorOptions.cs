namespace SnackCounter.Services;

public sealed class RefillOrchestratorOptions
{
    public TimeSpan AssignmentTtl { get; init; } = TimeSpan.FromHours(4);
    public TimeSpan PurchaseTtlAfterCommit { get; init; } = TimeSpan.FromHours(24);
}