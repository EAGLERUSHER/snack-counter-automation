namespace SnackCounter.Services;

using SnackCounter.Domain;
using SnackCounter.Ports;

public sealed record OfficeMonthlySummary(
    OfficeId OfficeId,
    decimal Spent,
    decimal Budget,
    Currency Currency,
    bool OverBudget);

public sealed record CompanyMonthlyReport(
    int Year,
    int Month,
    IReadOnlyList<OfficeMonthlySummary> Offices,
    decimal TotalInReportingCurrency,
    Currency ReportingCurrency);

public sealed class MonthlyReportService
{
    private readonly IBudgetLedger _ledger;
    private readonly IExchangeRateProvider _fx;

    public MonthlyReportService(IBudgetLedger ledger, IExchangeRateProvider fx)
    {
        _ledger = ledger;
        _fx = fx;
    }

    public CompanyMonthlyReport BuildReport(int year, int month, Currency reportingCurrency = Currency.NOK)
    {
        var offices = new List<OfficeMonthlySummary>();
        decimal total = 0;

        foreach (OfficeId office in Enum.GetValues<OfficeId>())
        {
            var spent = _ledger.GetSpendsForMonth(office, year, month).Sum(e => e.Amount);
            var budget = OfficeBudget.MonthlyBudget(office);
            var currency = OfficeBudget.GetCurrency(office);
            offices.Add(new OfficeMonthlySummary(office, spent, budget, currency, spent > budget));
            total += _fx.Convert(spent, currency, reportingCurrency);
        }

        return new CompanyMonthlyReport(year, month, offices, total, reportingCurrency);
    }
}