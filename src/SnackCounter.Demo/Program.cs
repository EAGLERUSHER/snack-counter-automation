using SnackCounter.Domain;
using SnackCounter.Infrastructure;
using SnackCounter.Services;
using SnackCounter.Simulators;

var clock = new FakeClock();
var volunteers = new InMemoryVolunteerRegistry();
volunteers.Register(new Employee { Id = "oslo-1", Name = "Ada", HomeOffice = OfficeId.Oslo });
volunteers.Register(new Employee { Id = "oslo-2", Name = "Bob", HomeOffice = OfficeId.Oslo });
volunteers.Register(new Employee { Id = "blr-1", Name = "Isha", HomeOffice = OfficeId.Bengaluru });

var ledger = new InMemoryBudgetLedger();
var budget = new BudgetService(ledger, clock);
var chat = new InMemoryChatNotifier(clock);
var hr = new InMemoryHrReimbursementClient();
var refills = new InMemoryRefillRepository();
var selector = new VolunteerSelector(volunteers, clock);
var orchestrator = new RefillOrchestrator(refills, volunteers, selector, budget, chat, hr, clock);
var reports = new MonthlyReportService(ledger, new FixedExchangeRateProvider());

static void Step(string label) => Console.WriteLine($"\n== {label} ==");

Step("1. Report empty snack box (Oslo)");
var refill = orchestrator.ReportEmpty(OfficeId.Oslo, "reporter-99");
Console.WriteLine($"Refill {refill.Id} → assigned to {refill.AssignedVolunteerId}");

Step("2. Volunteer commits");
var volunteer = refill.AssignedVolunteerId!;
orchestrator.CommitToRefill(refill.Id, volunteer);

Step("3. Record purchase (500 NOK)");
orchestrator.RecordPurchase(refill.Id, volunteer, 500m, Currency.NOK, new byte[] { 1, 2, 3 });

Step("4. Submit to HR & approve");
orchestrator.SubmitToHr(refill.Id);
orchestrator.AdvanceHrSimulation(refill.Id);

Step("5. Monthly report");
var report = reports.BuildReport(clock.UtcNow.Year, clock.UtcNow.Month);
foreach (var office in report.Offices)
{
    Console.WriteLine(
        $"{office.OfficeId}: spent {office.Spent} {office.Currency} / budget {office.Budget}");
}
Console.WriteLine($"Company total (NOK): {report.TotalInReportingCurrency}");

Step("Recent chat messages");
foreach (var msg in chat.Messages.TakeLast(5))
    Console.WriteLine($"  [{msg.At:HH:mm}] {msg.Target}: {msg.Text}");

Console.WriteLine("\nDone. Run tests with: dotnet test");