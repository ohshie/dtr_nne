using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.ExternalServices;

public interface IExternalServiceProvider
{
    IExternalService GetExistingInUseService(ExternalServiceType type);
    IExternalService ProvideService(ExternalServiceType type);
}