using SnackCounter.Domain;
using SnackCounter.Services;
using Xunit;

namespace SnackCounter.Tests;

public class ReassignmentTests
{
    [Fact]
    public void StaleAssignment_ReassignsDifferentVolunteer()
    {
        var kit = new TestKit(new RefillOrchestratorOptions { AssignmentTtl = TimeSpan.FromHours(4) });
        kit.SeedOsloVolunteers(("a1", "Ada"), ("b2", "Bob"));

        var refill = kit.Orchestrator.ReportEmpty(OfficeId.Oslo, "reporter");
        var first = refill.AssignedVolunteerId!;

        kit.Clock.Advance(TimeSpan.FromHours(5));
        var updated = kit.Orchestrator.ReassignIfStale(refill.Id);

        Assert.NotNull(updated);
        Assert.NotEqual(first, updated!.AssignedVolunteerId);
        Assert.Equal(RefillStatus.Assigned, updated.Status);
    }

    [Fact]
    public void OnlyCommittedVolunteer_CanRecordPurchase()
    {
        var kit = new TestKit();
        kit.SeedOsloVolunteers(("a1", "Ada"), ("b2", "Bob"));

        var refill = kit.Orchestrator.ReportEmpty(OfficeId.Oslo, "reporter");
        var assigned = refill.AssignedVolunteerId!;
        kit.Orchestrator.CommitToRefill(refill.Id, assigned);

        var ex = Assert.Throws<SnackCounterException>(() =>
            kit.Orchestrator.RecordPurchase(refill.Id, "b2", 500m, Currency.NOK, TestKit.FakeReceipt()));
        Assert.Equal(ErrorCodes.NotAuthorized, ex.ErrorCode);

        var ok = kit.Orchestrator.RecordPurchase(refill.Id, assigned, 500m, Currency.NOK, TestKit.FakeReceipt());
        Assert.Equal(RefillStatus.Purchased, ok.Status);
    }
}