namespace SnackCounter.Domain;

public static class OfficeBudget
{
    public static decimal MonthlyBudget(OfficeId office) => office switch
    {
        OfficeId.Oslo => 6_000m,
        OfficeId.Bengaluru => 15_000m,
        _ => throw new ArgumentOutOfRangeException(nameof(office))
    };

    public static Currency GetCurrency(OfficeId office) => office switch
    {
        OfficeId.Oslo => Currency.NOK,
        OfficeId.Bengaluru => Currency.INR,
        _ => throw new ArgumentOutOfRangeException(nameof(office))
    };

    public static (decimal Min, decimal Max) TypicalPurchaseRange(OfficeId office) => office switch
    {
        OfficeId.Oslo => (300m, 1_200m),
        OfficeId.Bengaluru => (500m, 3_000m),
        _ => throw new ArgumentOutOfRangeException(nameof(office))
    };
}