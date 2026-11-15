using SnackCounter.Domain;
using SnackCounter.Infrastructure;
using SnackCounter.Ports;
using SnackCounter.Services;
using SnackCounter.Simulators;

namespace SnackCounter.Api.Services;

public sealed class SnackCounterApplication
{
    public InMemoryRefillRepository Refills { get; } = new();
    public InMemoryVolunteerRegistry Volunteers { get; } = new();
    public InMemoryBudgetLedger Ledger { get; } = new();
    public FakeClock Clock { get; } = new();
    public InMemoryChatNotifier Chat { get; }
    public InMemoryHrReimbursementClient Hr { get; } = new();
    public BudgetService Budget { get; }
    public RefillOrchestrator Orchestrator { get; }
    public MonthlyReportService Reports { get; }

    public SnackCounterApplication()
    {
        Chat = new InMemoryChatNotifier(Clock);
        Budget = new BudgetService(Ledger, Clock);
        var selector = new VolunteerSelector(Volunteers, Clock);
        var fx = new FixedExchangeRateProvider();
        Reports = new MonthlyReportService(Ledger, fx);
        Orchestrator = new RefillOrchestrator(
            Refills, Volunteers, selector, Budget, Chat, Hr, Clock);
        SeedVolunteers();
    }

    private void SeedVolunteers()
    {
        foreach (var (id, name, office) in new[]
        {
            ("oslo-1", "Ada", OfficeId.Oslo),
            ("oslo-2", "Bob", OfficeId.Oslo),
            ("oslo-3", "Erik", OfficeId.Oslo),
            ("blr-1", "Isha", OfficeId.Bengaluru),
            ("blr-2", "Ravi", OfficeId.Bengaluru),
            ("blr-3", "Priya", OfficeId.Bengaluru),
        })
        {
            Volunteers.Register(new Employee { Id = id, Name = name, HomeOffice = office });
        }
    }
}