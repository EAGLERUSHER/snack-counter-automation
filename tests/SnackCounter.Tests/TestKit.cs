using SnackCounter.Domain;
using SnackCounter.Infrastructure;
using SnackCounter.Ports;
using SnackCounter.Services;
using SnackCounter.Simulators;

namespace SnackCounter.Tests;

public sealed class TestKit
{
    public InMemoryRefillRepository Refills { get; } = new();
    public InMemoryBudgetLedger Ledger { get; } = new();
    public InMemoryVolunteerRegistry Volunteers { get; } = new();
    public FakeClock Clock { get; } = new();
    public InMemoryChatNotifier Chat { get; }
    public InMemoryHrReimbursementClient Hr { get; } = new();
    public FixedExchangeRateProvider Fx { get; } = new();
    public RefillOrchestrator Orchestrator { get; }
    public BudgetService Budget { get; }
    public MonthlyReportService Reports { get; }

    public TestKit(RefillOrchestratorOptions? options = null)
    {
        Chat = new InMemoryChatNotifier(Clock);
        Budget = new BudgetService(Ledger, Clock);
        var selector = new VolunteerSelector(Volunteers, Clock);
        Reports = new MonthlyReportService(Ledger, Fx);
        Orchestrator = new RefillOrchestrator(
            Refills, Volunteers, selector, Budget, Chat, Hr, Clock, options);
    }

    public void SeedOsloVolunteers(params (string id, string name)[] people)
    {
        foreach (var (id, name) in people)
        {
            Volunteers.Register(new Employee
            {
                Id = id,
                Name = name,
                HomeOffice = OfficeId.Oslo
            });
        }
    }

    public void SeedBengaluruVolunteers(params (string id, string name)[] people)
    {
        foreach (var (id, name) in people)
        {
            Volunteers.Register(new Employee
            {
                Id = id,
                Name = name,
                HomeOffice = OfficeId.Bengaluru
            });
        }
    }

    public static byte[] FakeReceipt() => new byte[] { 0xFF, 0xD8, 0x01 };
}