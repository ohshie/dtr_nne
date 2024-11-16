using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.ExternalServices;

public interface IExternalServiceProvider
{
    Task<IExternalService> GetService(ExternalServiceType type);
}