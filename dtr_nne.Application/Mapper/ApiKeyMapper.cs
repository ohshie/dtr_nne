using dtr_nne.Application.DTO.Llm;
using dtr_nne.Application.DTO.Translator;
using dtr_nne.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace dtr_nne.Application.Mapper;

[Mapper]
public partial class ApiKeyMapper : IApiKeyMapper
{
    public partial TranslatorApi MapTranslatorApiDtoToTranslatorApi(TranslatorApiDto translatorApiDto);
    public partial LlmApi MapLlmApiDtoToLlmApi(LlmApiDto llmApiDto);
}