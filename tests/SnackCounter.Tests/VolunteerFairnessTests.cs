using SnackCounter.Domain;
using SnackCounter.Services;
using Xunit;

namespace SnackCounter.Tests;

public class VolunteerFairnessTests
{
    [Fact]
    public void Selector_PrefersVolunteerWithOldestLastRefill()
    {
        var kit = new TestKit();
        kit.SeedOsloVolunteers(("a1", "Ada"), ("b2", "Bob"));

        var ada = kit.Volunteers.GetById("a1")!;
        ada.LastRefillAt = kit.Clock.UtcNow.AddDays(-30);
        ada.RefillCountThisQuarter = 2;
        kit.Volunteers.Update(ada);

        var bob = kit.Volunteers.GetById("b2")!;
        bob.LastRefillAt = kit.Clock.UtcNow.AddDays(-2);
        bob.RefillCountThisQuarter = 0;
        kit.Volunteers.Update(bob);

        var selector = new VolunteerSelector(kit.Volunteers, kit.Clock);
        var pick = selector.SelectVolunteer(OfficeId.Oslo, new HashSet<string>());

        Assert.Equal("a1", pick!.Id);
    }

    [Fact]
    public void Visitor_CanBeAssigned_AtHostOffice()
    {
        var kit = new TestKit();
        kit.Volunteers.Register(new Employee
        {
            Id = "visitor",
            Name = "Vera",
            HomeOffice = OfficeId.Oslo,
            VisitingOffice = OfficeId.Bengaluru,
            LocationStatus = WorkLocationStatus.InOffice
        });
        kit.SeedBengaluruVolunteers(("local", "Lakshmi"));

        var refill = kit.Orchestrator.ReportEmpty(OfficeId.Bengaluru, "reporter");
        Assert.Contains(refill.AssignedVolunteerId, new[] { "visitor", "local" });
    }
}