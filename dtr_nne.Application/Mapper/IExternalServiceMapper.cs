using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.DTO.Llm;
using dtr_nne.Application.DTO.Translator;
using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.Mapper;

public interface IExternalServiceMapper
{
    public ExternalService DtoToService(ExternalServiceDto externalServiceDto);
    public ExternalServiceDto ServiceToDto(ExternalService service);
    public ExternalService BaseDtoToService(BaseExternalServiceDto externalServiceDto);
    public ExternalServiceDto ServiceToBaseDto(ExternalService service);
    public LlmApi MapLlmApiDtoToLlmApi(LlmApiDto llmApiDto);
}