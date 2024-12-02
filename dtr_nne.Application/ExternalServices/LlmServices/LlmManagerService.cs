using System.Runtime.CompilerServices;
using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;

[assembly: InternalsVisibleTo("Tests")]

namespace dtr_nne.Application.ExternalServices.LlmServices;

public class LlmManagerService(
    ILogger<LlmManagerService> logger,
    IExternalServiceProviderRepository repository,
    IExternalServiceMapper mapper,
    IExternalServiceProvider serviceProvider) : ILlmManagerService
{
    public async Task<ErrorOr<ExternalServiceDto>> Add(ExternalServiceDto service)
    {
        logger.LogInformation("Starting Add method for service: {Service}", service.ApiKey);
        
        var mappedApiKey = mapper.DtoToService(service);
        
        var verifiedKey = await CheckApiKey(mappedApiKey);
        if (verifiedKey.IsError)
        {
            return verifiedKey.FirstError;
        }

        var success = await repository.Add(verifiedKey.Value);
        if (!success)
        {
            logger.LogError("Failed to add External service to repository");
        }
        
        return service;
    }

    public Task<ErrorOr<ExternalServiceDto>> UpdateKey(ExternalServiceDto service)
    {
        throw new NotImplementedException();
    }
    
    internal async Task<ErrorOr<ExternalService>> CheckApiKey(ExternalService service)
    {
        var llmService = await serviceProvider.GetService(ExternalServiceType.Llm) as ILlmService;
        if (llmService is null)
        {
            return Errors.ExternalServiceProvider.Service.NoSavedApiKeyFound;
        }

        var aiAssistant = new InternalAiAssistant
        {
            ApiKey = service.ApiKey,
        };
        
        var testArticle = new Article { OriginalBody = "test" };
        var verifiedKey = await llmService.ProcessArticleAsync(testArticle, aiAssistant);
        if (verifiedKey.IsError)
        {
            return verifiedKey.FirstError == Errors.ExternalServiceProvider.Llm.AssistantRunError 
                ? Errors.ExternalServiceProvider.Llm.AssistantRunError 
                : Errors.ExternalServiceProvider.Service.BadApiKey;
        }
        
        return service;
    }
}