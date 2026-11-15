using SnackCounter.Domain;
using Xunit;

namespace SnackCounter.Tests;

public class HrLifecycleTests
{
    [Fact]
    public void FullLifecycle_EndToEnd_ReimbursedUpdatesVolunteerStats()
    {
        var kit = new TestKit();
        kit.SeedOsloVolunteers(("a1", "Ada"));

        var refill = kit.Orchestrator.ReportEmpty(OfficeId.Oslo, "reporter");
        var ada = refill.AssignedVolunteerId!;
        kit.Orchestrator.CommitToRefill(refill.Id, ada);
        kit.Orchestrator.RecordPurchase(refill.Id, ada, 450m, Currency.NOK, TestKit.FakeReceipt());
        kit.Orchestrator.SubmitToHr(refill.Id);

        var submitted = kit.Refills.GetById(refill.Id)!;
        Assert.NotNull(submitted.HrClaimId);
        var claim = kit.Hr.GetClaim(submitted.HrClaimId!)!;
        Assert.Equal(450m, claim.Amount);
        Assert.Equal(ada, claim.EmployeeId);

        kit.Orchestrator.AdvanceHrSimulation(refill.Id);
        var done = kit.Refills.GetById(refill.Id)!;
        Assert.Equal(RefillStatus.Reimbursed, done.Status);

        var employee = kit.Volunteers.GetById(ada)!;
        Assert.Equal(1, employee.RefillCountThisQuarter);
        Assert.NotNull(employee.LastRefillAt);
        Assert.Null(employee.OpenRefillCommitmentId);
    }
}