using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.ExternalServices.TranslatorServices;

public class TranslatorApiKeyService( 
    IExternalServiceMapper mapper,
    IExternalServiceProvider provider,
    IExternalServiceProviderRepository repository,
    IUnitOfWork<INneDbContext> unitOfWork, 
    ILogger<TranslatorApiKeyService> logger) : ITranslatorApiKeyService
{
    private readonly List<Headline> _testHeadlines = [new Headline{OriginalHeadline = "api test"}];
    
    public async Task<ErrorOr<ExternalServiceDto>> Add(ExternalServiceDto serviceDto)
    {
        logger.LogInformation("Starting Add method for service: {Service}", serviceDto.ApiKey);
        
        var mappedApiKey = MapApiKeys(serviceDto);
        
        var validKey = await CheckKeyValidity(mappedApiKey);
        if (validKey.IsError)
        {
            return validKey.FirstError;
        }
        
        var success = await repository.Add(mappedApiKey);
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
        
        return serviceDto;
    }

    public async Task<ErrorOr<ExternalServiceDto>> UpdateKey(ExternalServiceDto serviceDto)
    {
        logger.LogInformation("Starting Update method for service {Service}", serviceDto.ServiceName);
        var requiredServiceExist = FindRequiredExistingService(serviceDto);
        if (requiredServiceExist.IsError)
        {
            return requiredServiceExist.FirstError;
        }
        
        var serviceToUpdate = requiredServiceExist.Value;
        
        var mappedIncomingService = mapper.DtoToService(serviceDto);
        
        if (mappedIncomingService.ApiKey == serviceToUpdate.ApiKey)
        {
            var validKey = await CheckKeyValidity(mappedIncomingService);
            if (validKey.IsError)
            {
                return validKey.FirstError;
            }
        }
        
        mappedIncomingService.Id = serviceToUpdate.Id;
        serviceToUpdate = mappedIncomingService;
        
        var success = repository.Update(serviceToUpdate);
        if (!success)
        {
            logger.LogError("Failed to update External service in repository");
            return Errors.DbErrors.AddingToDbFailed;
        }
        
        success = await unitOfWork.Save();
        if (!success)
        {
            logger.LogError("Failed to save API key in unit of work");
            return Errors.DbErrors.UnitOfWorkSaveFailed;
        }

        return serviceDto;
    }

    internal async Task<ErrorOr<bool>> CheckKeyValidity(ExternalService incomingService)
    {
        var translatorService = provider.Provide(incomingService.Type, incomingService.ApiKey) as ITranslatorService;
        if (translatorService is null)
        {
            return Errors.ExternalServiceProvider.Service.NoSavedServiceFound;
        }
        
        var validKey = await translatorService.Translate(_testHeadlines);
        if (validKey.IsError)
        {
            logger.LogWarning("Failed to validate API key. Error: {Error}", validKey.FirstError);
            return validKey.FirstError;
        }
        
        logger.LogDebug("API key validated successfully");
        return true;
    }
    
    internal ErrorOr<ExternalService> FindRequiredExistingService(ExternalServiceDto serviceDto)
    {
        if (repository.GetByType(serviceDto.Type) is not { } currentServices)
        {
            return Errors.ExternalServiceProvider.Service.NoSavedServiceFound;
        }

        if (currentServices.Find(s => s.ServiceName == serviceDto.ServiceName) is not { } serviceToUpdate)
        {
            return Errors.ExternalServiceProvider.Service.NoSavedServiceFound;
        }

        return serviceToUpdate;
    }
        
    internal ExternalService MapApiKeys(ExternalServiceDto service)
    {
        var mappedApiKey = mapper.DtoToService(service);
        logger.LogDebug("Mapped TranslatorApiDto to TranslatorApi entity");

        return mappedApiKey;
    }
}