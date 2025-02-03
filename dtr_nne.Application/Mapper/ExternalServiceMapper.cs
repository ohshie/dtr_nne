using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace dtr_nne.Application.Mapper;

[Mapper]
public partial class ExternalServiceMapper : IExternalServiceMapper
{
    public partial ExternalService DtoToService(ExternalServiceDto externalServiceDto);
    public partial ExternalService DtoToService(BaseExternalServiceDto baseExternalServiceDto);
    public partial List<ExternalService> DtoToService(List<ExternalServiceDto> externalServiceDto);
    public partial ExternalServiceDto ServiceToDto(ExternalService service);
    public partial List<ExternalServiceDto> ServiceToDto(List<ExternalService> service);
}