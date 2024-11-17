using dtr_nne.Application.DTO.Llm;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.ExternalServices.LlmServices;

public class LlmApiKeyService(ILogger<LlmApiKeyService> logger, IExternalServiceMapper mapper, IExternalServiceProvider llmServiceProvider) : ILlmApiKeyService
{
    public async Task<ErrorOr<LlmApiDto>> Add(LlmApiDto apiKey)
    {
        var mappedApiKey = MapApiKeys(apiKey);
        var verifiedKey = await CheckApiKey(mappedApiKey);
        return apiKey;
    }

    public Task<ErrorOr<LlmApiDto>> UpdateKey(LlmApiDto apiKey)
    {
        throw new NotImplementedException();
    }

    private async Task<ErrorOr<LlmApi>> CheckApiKey(LlmApi apiKey)
    {
        var llmService = await llmServiceProvider.GetService(ExternalServiceType.Llm) as ILlmService;
        var testArticle = new Article { Body = "test" };
        var verifiedKey = await llmService.RewriteAsync(testArticle, apiKey);
        return apiKey;
    }
    
    private LlmApi MapApiKeys(LlmApiDto apiKey)
    {
        var mappedApiKey = mapper.MapLlmApiDtoToLlmApi(apiKey);
        logger.LogDebug("Mapped TranslatorApiDto to TranslatorApi entity");

        return mappedApiKey;
    }
}