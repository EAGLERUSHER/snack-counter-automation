using SnackCounter.Domain;
using Xunit;

namespace SnackCounter.Tests;

public class RefillConcurrencyTests
{
    [Fact]
    public void TwoCommits_OnSameRefill_SecondFails()
    {
        var kit = new TestKit();
        kit.SeedOsloVolunteers(("a1", "Ada"), ("b2", "Bob"));

        var refill = kit.Orchestrator.ReportEmpty(OfficeId.Oslo, "reporter");
        var assigned = refill.AssignedVolunteerId!;

        kit.Orchestrator.CommitToRefill(refill.Id, assigned);

        var ex = Assert.Throws<SnackCounterException>(() =>
            kit.Orchestrator.CommitToRefill(refill.Id, "b2"));
        Assert.Equal(ErrorCodes.Conflict, ex.ErrorCode);
    }

    [Fact]
    public void ReportEmpty_Twice_UsesSingleActiveRefill()
    {
        var kit = new TestKit();
        kit.SeedOsloVolunteers(("a1", "Ada"));

        var first = kit.Orchestrator.ReportEmpty(OfficeId.Oslo, "r1");
        var second = kit.Orchestrator.ReportEmpty(OfficeId.Oslo, "r2");

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(first.Id, kit.Refills.GetActiveByOffice(OfficeId.Oslo)!.Id);
    }

    [Fact]
    public void WrongVolunteer_CannotCommit()
    {
        var kit = new TestKit();
        kit.SeedOsloVolunteers(("a1", "Ada"), ("b2", "Bob"));

        var refill = kit.Orchestrator.ReportEmpty(OfficeId.Oslo, "reporter");

        var ex = Assert.Throws<SnackCounterException>(() =>
            kit.Orchestrator.CommitToRefill(refill.Id, "b2"));
        Assert.Equal(ErrorCodes.NotAuthorized, ex.ErrorCode);
    }
}