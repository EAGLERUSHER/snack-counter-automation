using SnackCounter.Domain;
using Xunit;

namespace SnackCounter.Tests;

public class BudgetAndReportTests
{
    [Fact]
    public void BudgetBlocksSpend_WhenMonthlyCapExceeded()
    {
        var kit = new TestKit();
        kit.SeedOsloVolunteers(("a1", "Ada"));

        CompleteRefill(kit, OfficeId.Oslo, 5_500m, Currency.NOK);

        var refill = kit.Orchestrator.ReportEmpty(OfficeId.Oslo, "r2");
        kit.Orchestrator.CommitToRefill(refill.Id, refill.AssignedVolunteerId!);

        var ex = Assert.Throws<SnackCounterException>(() =>
            kit.Orchestrator.RecordPurchase(refill.Id, refill.AssignedVolunteerId!, 600m, Currency.NOK, TestKit.FakeReceipt()));
        Assert.Equal(ErrorCodes.BudgetExceeded, ex.ErrorCode);
    }

    [Fact]
    public void MonthlyReport_AggregatesBothOffices_WithFx()
    {
        var kit = new TestKit();
        kit.SeedOsloVolunteers(("o1", "Ola"));
        kit.SeedBengaluruVolunteers(("i1", "Isha"));

        CompleteRefill(kit, OfficeId.Oslo, 1_000m, Currency.NOK);
        CompleteRefill(kit, OfficeId.Bengaluru, 2_000m, Currency.INR);

        var report = kit.Reports.BuildReport(kit.Clock.UtcNow.Year, kit.Clock.UtcNow.Month);

        var oslo = report.Offices.Single(o => o.OfficeId == OfficeId.Oslo);
        var blr = report.Offices.Single(o => o.OfficeId == OfficeId.Bengaluru);

        Assert.Equal(1_000m, oslo.Spent);
        Assert.Equal(6_000m, oslo.Budget);
        Assert.Equal(2_000m, blr.Spent);
        Assert.Equal(15_000m, blr.Budget);

        // 1000 NOK + 2000 INR -> NOK: 1000 + 2000/8 = 1250
        Assert.Equal(1_250m, report.TotalInReportingCurrency);
        Assert.Equal(Currency.NOK, report.ReportingCurrency);
    }

    [Fact]
    public void OverBudget_FlaggedInReport()
    {
        var kit = new TestKit();
        kit.SeedOsloVolunteers(("o1", "Ola"));
        // Force ledger entries without orchestrator cap (direct append) to simulate overrun after manual adjustment
        kit.Budget.RecordSpend(OfficeId.Oslo, 6_500m, Currency.NOK, "manual");

        var report = kit.Reports.BuildReport(kit.Clock.UtcNow.Year, kit.Clock.UtcNow.Month);
        var oslo = report.Offices.Single(o => o.OfficeId == OfficeId.Oslo);
        Assert.True(oslo.OverBudget);
        Assert.True(kit.Budget.IsOverBudget(OfficeId.Oslo));
    }

    private static void CompleteRefill(TestKit kit, OfficeId office, decimal amount, Currency currency)
    {
        var reporter = "rep";
        var refill = kit.Orchestrator.ReportEmpty(office, reporter);
        var volunteer = refill.AssignedVolunteerId!;
        kit.Orchestrator.CommitToRefill(refill.Id, volunteer);
        kit.Orchestrator.RecordPurchase(refill.Id, volunteer, amount, currency, TestKit.FakeReceipt());
        kit.Orchestrator.SubmitToHr(refill.Id);
        kit.Orchestrator.AdvanceHrSimulation(refill.Id);
    }
}