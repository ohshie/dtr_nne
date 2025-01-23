using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.ExternalServices;

public interface IExternalServiceProvider
{
    IExternalService Provide(ExternalServiceType type, string apiKey = "");
}