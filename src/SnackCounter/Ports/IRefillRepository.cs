namespace SnackCounter.Ports;

using SnackCounter.Domain;

public interface IRefillRepository
{
    void Add(RefillRequest refill);
    RefillRequest? GetById(string id);
    RefillRequest? GetActiveByOffice(OfficeId officeId);
    void Update(RefillRequest refill);
    IReadOnlyList<RefillRequest> ListAll();
}