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
    ITranslatorApiRepository repository,
    IUnitOfWork<INneDbContext> unitOfWork, 
    ILogger<TranslatorApiKeyService> logger) : ITranslatorApiKeyService
{
    private readonly List<Headline> _testHeadlines = [new Headline{OriginalHeadline = "api test"}];
    
    public async Task<ErrorOr<TranslatorApiDto>> Add(TranslatorApiDto apiKey)
    {
        logger.LogInformation("Starting Add method for API key: {ApiKey}", apiKey.ApiKey);
        
        var mappedApiKey = MapApiKeys(apiKey);
        var verifiedKey = await CheckApiKey(mappedApiKey);
        if (verifiedKey.IsError)
        { 
            return verifiedKey.FirstError;
        }
        
        var success = await repository.Add(verifiedKey.Value);
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
        var currentKey = await repository.Get(1);
        if (currentKey is null)
        {
            logger.LogError("No current key found in Db, cannot update key");
            return Errors.Translator.Service.NoSavedApiKeyFound;
        }
        
        var mappedApiKey = MapApiKeys(apiKey);
        var verifiedKey = await CheckApiKey(mappedApiKey);
        if (verifiedKey.IsError)
        { 
            return verifiedKey.FirstError;
        }

        currentKey.ApiKey = verifiedKey.Value.ApiKey;
        
        var success = repository.Update(currentKey);
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
    
    private async Task<ErrorOr<TranslatorApi>> CheckApiKey(TranslatorApi apiKey)
    {
        var validKey = await translatorService.Translate(_testHeadlines, apiKey);
        if (validKey.IsError)
        {
            logger.LogWarning("Failed to validate API key. Error: {Error}", validKey.FirstError);
            return validKey.FirstError;
        }
        
        logger.LogDebug("API key validated successfully");
        return apiKey;
    }

    private TranslatorApi MapApiKeys(TranslatorApiDto apiKey)
    {
        var mappedApiKey = apiKeyMapper.MapTranslatorApiDtoToTranslatorApi(apiKey);
        logger.LogDebug("Mapped TranslatorApiDto to TranslatorApi entity");

        return mappedApiKey;
    }
}