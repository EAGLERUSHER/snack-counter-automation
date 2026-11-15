namespace SnackCounter.Ports;

using SnackCounter.Domain;

public interface IVolunteerRegistry
{
    IReadOnlyList<Employee> GetVolunteersForOffice(OfficeId officeId);
    Employee? GetById(string employeeId);
    void Update(Employee employee);
}