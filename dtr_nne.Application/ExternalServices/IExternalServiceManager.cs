using dtr_nne.Application.DTO.ExternalService;

namespace dtr_nne.Application.ExternalServices;

public interface IExternalServiceManager
{
    public Task<ErrorOr<ExternalServiceDto>> Add(ExternalServiceDto service);
    public Task<ErrorOr<ExternalServiceDto>> Update(ExternalServiceDto service);
}