namespace SnackCounter.Ports;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}