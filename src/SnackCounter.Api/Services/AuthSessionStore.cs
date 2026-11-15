namespace SnackCounter.Api.Services;

public sealed class AuthSessionStore
{
    private readonly Dictionary<string, string> _tokenToEmployee = new();
    private readonly object _lock = new();

    public string CreateSession(string employeeId)
    {
        var token = Guid.NewGuid().ToString("N");
        lock (_lock)
            _tokenToEmployee[token] = employeeId;
        return token;
    }

    public bool TryGetEmployee(string token, out string employeeId)
    {
        lock (_lock)
            return _tokenToEmployee.TryGetValue(token, out employeeId!);
    }

    public void Revoke(string token)
    {
        lock (_lock)
            _tokenToEmployee.Remove(token);
    }
}