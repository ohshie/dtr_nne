using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.ExternalServices.LlmServices;

public interface ILlmManagerService
{
    Task<ErrorOr<ExternalServiceDto>> Add(ExternalServiceDto service);
    Task<ErrorOr<ExternalServiceDto>> UpdateKey(ExternalServiceDto service);
}