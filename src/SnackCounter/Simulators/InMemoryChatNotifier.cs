namespace SnackCounter.Simulators;

using SnackCounter.Ports;

public sealed class InMemoryChatNotifier : IChatNotifier
{
    private readonly List<ChatMessage> _messages = new();
    private readonly IClock _clock;

    public InMemoryChatNotifier(IClock clock) => _clock = clock;

    public IReadOnlyList<ChatMessage> Messages => _messages;

    public void NotifyChannel(string officeChannel, string message) =>
        _messages.Add(new ChatMessage(officeChannel, message, _clock.UtcNow));

    public void NotifyEmployee(string employeeId, string message) =>
        _messages.Add(new ChatMessage($"dm:{employeeId}", message, _clock.UtcNow));
}