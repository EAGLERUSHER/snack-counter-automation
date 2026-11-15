namespace SnackCounter.Simulators;

using SnackCounter.Ports;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}