using dtr_nne.Application.DTO.Llm;
using dtr_nne.Application.DTO.Translator;
using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.Mapper;

public interface IApiKeyMapper
{
    public TranslatorApi MapTranslatorApiDtoToTranslatorApi(TranslatorApiDto translatorApiDto);
    public LlmApi MapLlmApiDtoToLlmApi(LlmApiDto llmApiDto);
}