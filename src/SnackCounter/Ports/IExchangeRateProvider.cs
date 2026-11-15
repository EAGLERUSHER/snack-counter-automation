namespace SnackCounter.Ports;

using SnackCounter.Domain;

public interface IExchangeRateProvider
{
  decimal Convert(decimal amount, Currency from, Currency to);
}