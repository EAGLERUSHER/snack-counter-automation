namespace SnackCounter.Simulators;

using SnackCounter.Domain;
using SnackCounter.Ports;

/// <summary>
/// Test/simulation rates: 1 NOK = 8 INR (not production FX).
/// </summary>
public sealed class FixedExchangeRateProvider : IExchangeRateProvider
{
    public decimal Convert(decimal amount, Currency from, Currency to)
    {
        if (from == to) return amount;
        if (from == Currency.NOK && to == Currency.INR) return amount * 8m;
        if (from == Currency.INR && to == Currency.NOK) return amount / 8m;
        throw new NotSupportedException($"Conversion {from} -> {to}");
    }
}