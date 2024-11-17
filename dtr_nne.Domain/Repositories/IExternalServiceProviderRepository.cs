using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;

namespace dtr_nne.Domain.Repositories;

public interface IExternalServiceProviderRepository : IRepository<ExternalService>
{
    public List<ExternalService>? GetByType(ExternalServiceType type);
}