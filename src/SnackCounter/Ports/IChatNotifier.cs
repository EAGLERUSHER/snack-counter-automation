namespace SnackCounter.Ports;

public interface IChatNotifier
{
    void NotifyChannel(string officeChannel, string message);
    void NotifyEmployee(string employeeId, string message);
    IReadOnlyList<ChatMessage> Messages { get; }
}

public sealed record ChatMessage(string Target, string Text, DateTimeOffset At);