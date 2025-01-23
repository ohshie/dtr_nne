using dtr_nne.Application.DTO.ExternalService;

namespace dtr_nne.Application.Services.ExternalServices;

public interface IExternalServiceManager
{
    public Task<ErrorOr<ExternalServiceDto>> Add(ExternalServiceDto serviceDto);
    public Task<ErrorOr<ExternalServiceDto>> Update(ExternalServiceDto serviceDto);
}