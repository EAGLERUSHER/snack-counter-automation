namespace SnackCounter.Infrastructure;

using SnackCounter.Domain;
using SnackCounter.Ports;

public sealed class InMemoryVolunteerRegistry : IVolunteerRegistry
{
    private readonly Dictionary<string, Employee> _employees = new();

    public void Register(Employee employee) => _employees[employee.Id] = employee;

    public IReadOnlyList<Employee> GetVolunteersForOffice(OfficeId officeId)
    {
        return _employees.Values
            .Where(e => e.HomeOffice == officeId ||
                        (e.VisitingOffice == officeId && e.LocationStatus != WorkLocationStatus.Holiday))
            .Select(Clone)
            .ToList();
    }

    public Employee? GetById(string employeeId) =>
        _employees.TryGetValue(employeeId, out var e) ? Clone(e) : null;

    public void Update(Employee employee) => _employees[employee.Id] = Clone(employee);

    private static Employee Clone(Employee e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        HomeOffice = e.HomeOffice,
        LocationStatus = e.LocationStatus,
        VisitingOffice = e.VisitingOffice,
        LastRefillAt = e.LastRefillAt,
        RefillCountThisQuarter = e.RefillCountThisQuarter,
        OpenRefillCommitmentId = e.OpenRefillCommitmentId
    };
}