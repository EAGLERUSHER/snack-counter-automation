namespace SnackCounter.Ports;

using SnackCounter.Domain;

public interface IBudgetLedger
{
    void Append(SpendEvent spend);
    IReadOnlyList<SpendEvent> GetSpendsForMonth(OfficeId office, int year, int month);
    IReadOnlyList<SpendEvent> GetAllSpendsForMonth(int year, int month);
}