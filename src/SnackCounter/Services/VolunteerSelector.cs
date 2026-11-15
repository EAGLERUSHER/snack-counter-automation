namespace SnackCounter.Services;

using SnackCounter.Domain;
using SnackCounter.Ports;

public sealed class VolunteerSelector
{
    private readonly IVolunteerRegistry _registry;
    private readonly IClock _clock;

    public VolunteerSelector(IVolunteerRegistry registry, IClock clock)
    {
        _registry = registry;
        _clock = clock;
    }

    public Employee? SelectVolunteer(OfficeId officeId, IReadOnlySet<string> excludeEmployeeIds)
    {
        var candidates = _registry.GetVolunteersForOffice(officeId)
            .Where(e => e.LocationStatus != WorkLocationStatus.Holiday)
            .Where(e => e.OpenRefillCommitmentId is null)
            .Where(e => !excludeEmployeeIds.Contains(e.Id))
            .Where(e => IsAvailableForOffice(e, officeId))
            .ToList();

        if (candidates.Count == 0)
            return null;

        return candidates
            .OrderBy(e => e.LocationStatus == WorkLocationStatus.InOffice ? 0 : 1)
            .ThenBy(e => e.LastRefillAt ?? DateTimeOffset.MinValue)
            .ThenBy(e => e.RefillCountThisQuarter)
            .First();
    }

    private bool IsAvailableForOffice(Employee e, OfficeId officeId)
    {
        if (e.HomeOffice == officeId)
            return true;
        return e.VisitingOffice == officeId;
    }
}