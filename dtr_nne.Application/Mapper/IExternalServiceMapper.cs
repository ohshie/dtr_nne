using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.Mapper;

public interface IExternalServiceMapper
{
    public ExternalService DtoToService(ExternalServiceDto externalServiceDto);
    public List<ExternalService> DtoToService(List<ExternalServiceDto> externalServiceDto);
    public ExternalServiceDto ServiceToDto(ExternalService service);
    public List<ExternalServiceDto> ServiceToDto(List<ExternalService> service);
}