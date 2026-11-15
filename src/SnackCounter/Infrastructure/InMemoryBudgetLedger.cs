namespace SnackCounter.Infrastructure;

using SnackCounter.Domain;
using SnackCounter.Ports;

public sealed class InMemoryBudgetLedger : IBudgetLedger
{
    private readonly List<SpendEvent> _events = new();
    private readonly object _lock = new();

    public void Append(SpendEvent spend)
    {
        lock (_lock)
        {
            _events.Add(spend);
        }
    }

    public IReadOnlyList<SpendEvent> GetSpendsForMonth(OfficeId office, int year, int month)
    {
        lock (_lock)
        {
            return _events
                .Where(e => e.OfficeId == office && e.Timestamp.Year == year && e.Timestamp.Month == month)
                .ToList();
        }
    }

    public IReadOnlyList<SpendEvent> GetAllSpendsForMonth(int year, int month)
    {
        lock (_lock)
        {
            return _events
                .Where(e => e.Timestamp.Year == year && e.Timestamp.Month == month)
                .ToList();
        }
    }
}