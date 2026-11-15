namespace SnackCounter.Simulators;

using SnackCounter.Ports;

public sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; private set; }

    public FakeClock(DateTimeOffset? start = null)
    {
        UtcNow = start ?? new DateTimeOffset(2025, 6, 1, 9, 0, 0, TimeSpan.Zero);
    }

    public void Advance(TimeSpan delta) => UtcNow = UtcNow.Add(delta);
    public void Set(DateTimeOffset instant) => UtcNow = instant;
}