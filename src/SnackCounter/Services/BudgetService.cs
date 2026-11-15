namespace SnackCounter.Services;

using SnackCounter.Domain;
using SnackCounter.Ports;

public sealed class BudgetService
{
    private readonly IBudgetLedger _ledger;
    private readonly IClock _clock;

    public BudgetService(IBudgetLedger ledger, IClock clock)
    {
        _ledger = ledger;
        _clock = clock;
    }

    public decimal GetSpentThisMonth(OfficeId office)
    {
        var now = _clock.UtcNow;
        return _ledger.GetSpendsForMonth(office, now.Year, now.Month)
            .Sum(e => e.Amount);
    }

    public bool CanSpend(OfficeId office, decimal additionalAmount)
    {
        var spent = GetSpentThisMonth(office);
        var budget = OfficeBudget.MonthlyBudget(office);
        return spent + additionalAmount <= budget;
    }

    public bool IsOverBudget(OfficeId office) =>
        GetSpentThisMonth(office) > OfficeBudget.MonthlyBudget(office);

    public void RecordSpend(OfficeId office, decimal amount, Currency currency, string refillId)
    {
        if (currency != OfficeBudget.GetCurrency(office))
            throw new SnackCounterException($"Currency {currency} does not match office {office}");

        var evt = new SpendEvent(
            Guid.NewGuid().ToString("N"),
            office,
            amount,
            currency,
            refillId,
            _clock.UtcNow);
        _ledger.Append(evt);
    }
}