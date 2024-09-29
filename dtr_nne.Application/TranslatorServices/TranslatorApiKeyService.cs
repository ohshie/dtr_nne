using dtr_nne.Domain.Entities;
using dtr_nne.Application.DTO.Translator;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.TranslatorServices;

public class TranslatorApiKeyService(ITranslatorService translatorService, 
    IApiKeyMapper apiKeyMapper,
    IRepository<TranslatorApi> repository,
    IUnitOfWork<INneDbContext> unitOfWork, 
    ILogger<TranslatorApiKeyService> logger) : ITranslatorApiKeyService
{
    private readonly List<Headline> _testHeadlines = [new Headline()];
    
    public async Task<ErrorOr<TranslatorApiDto>> Add(TranslatorApiDto apiKey)
    {
        logger.LogInformation("Starting Add method for API key: {ApiKey}", apiKey.ApiKey);
        
        var mappedApiKey = await MapAndCheckApiKey(apiKey);
        if (mappedApiKey.IsError)
        { 
            return mappedApiKey.FirstError;
        }
        
        var success = await repository.Add(mappedApiKey.Value);
        if (!success)
        {
            logger.LogError("Failed to add API key to repository");
            return Errors.Translator.Api.AddingFailed;
        }
        
        logger.LogDebug("API key added to repository");

        var keySaved = await unitOfWork.Save();
        if (!keySaved)
        {
            logger.LogError("Failed to save API key in unit of work");
            return Errors.DbErrors.UnitOfWorkSaveFailed;
        }
        
        logger.LogInformation("API key added and saved successfully");
        
        return apiKey;
    }

    public async Task<ErrorOr<TranslatorApiDto>> UpdateKey(TranslatorApiDto apiKey)
    {
        var mappedApiKey = await MapAndCheckApiKey(apiKey);
        if (mappedApiKey.IsError)
        { 
            return mappedApiKey.FirstError;
        }
        
        var success = await repository.Update(mappedApiKey.Value);
        if (!success)
        {
            logger.LogError("Failed to update API key");
            return Errors.Translator.Api.UpdatingFailed;
        }
        
        var keySaved = await unitOfWork.Save();
        if (!keySaved)
        {
            logger.LogError("Failed to save changes to Db");
            return Errors.DbErrors.UnitOfWorkSaveFailed;
        }
        
        return apiKey;
    }
    
    private async Task<ErrorOr<TranslatorApi>> MapAndCheckApiKey(TranslatorApiDto apiKey)
    {
        var mappedApiKey = apiKeyMapper.MapTranslatorApiDtoToTranslatorApi(apiKey);
        logger.LogDebug("Mapped TranslatorApiDto to TranslatorApi entity");
        
        var validKey = await translatorService.Translate(_testHeadlines, mappedApiKey);
        if (validKey.IsError)
        {
            logger.LogWarning("Failed to validate API key. Error: {Error}", validKey.FirstError);
            return validKey.FirstError;
        }
        
        logger.LogDebug("API key validated successfully");
        return mappedApiKey;
    }
}