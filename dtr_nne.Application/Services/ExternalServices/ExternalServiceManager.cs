using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.Services.ExternalServices;

public class ExternalServiceManager(ILogger<ExternalServiceManager> logger,
    IExternalServiceProviderRepository repository,
    IUnitOfWork<INneDbContext> unitOfWork,
    IExternalServiceMapper mapper,
    IExternalServiceProvider serviceProvider) : IExternalServiceManager
{
    public async Task<ErrorOr<ExternalServiceDto>> Add(ExternalServiceDto serviceDto)
    {
        logger.LogInformation("Starting Add method for service: {Service}", serviceDto.ServiceName);

        var mappedIncomingService = mapper.DtoToService(serviceDto);
        
        var validService = await CheckKeyValidity(mappedIncomingService);
        if (validService.IsError)
        {
            return validService.FirstError;
        }
        
        var success = await repository.Add(mappedIncomingService);
        if (!success)
        {
            logger.LogError("Failed to add External service to repository");
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

    public async Task<ErrorOr<ExternalServiceDto>> Update(ExternalServiceDto serviceDto)
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
    
    internal async Task<ErrorOr<bool>> CheckKeyValidity(ExternalService incomingService)
    {
        if (serviceProvider.Provide(incomingService.Type, incomingService.ApiKey) is not { } externalService)
        {
            return Errors.ExternalServiceProvider.Service.NoSavedServiceFound;
        }

        ErrorOr<bool> success;
        switch (incomingService.Type)
        {
            case ExternalServiceType.Llm:
                success = await CheckLlmApiKey((externalService as ILlmService)!);
                if (success.IsError)
                {
                    return success.FirstError;
                }
                break;
            case ExternalServiceType.Translator:
                success = await CheckTranslatorKey((externalService as ITranslatorService)!, incomingService.ApiKey);
                if (success.IsError)
                {
                    return success.FirstError;
                }
                break;
            case ExternalServiceType.Scraper:
                success = await CheckScraperKey((externalService as IScrapingService)!);
                if (success.IsError)
                {
                    return success.FirstError;
                }
                break;
            default:
                return Errors.ExternalServiceProvider.Service.NoSavedServiceFound;
        }
        
        return success.Value;
    }
    
    internal async Task<ErrorOr<bool>> CheckLlmApiKey(ILlmService llmService)
    {
        logger.LogInformation("Starting to check api key for llm service");
        
        var testArticle = new ArticleContent { Body = "test" };
        var validKey = await llmService.ProcessArticleAsync(testArticle);
        if (validKey.IsError)
        {
            return validKey.FirstError == Errors.ExternalServiceProvider.Llm.AssistantRunError 
                ? Errors.ExternalServiceProvider.Llm.AssistantRunError 
                : Errors.ExternalServiceProvider.Service.BadApiKey;
        }
        
        return true;
    }
    
    internal async Task<ErrorOr<bool>> CheckTranslatorKey(ITranslatorService service, string incomingApiKey)
    {
        logger.LogInformation("Starting to check api key for translator service");
        
        List<Headline> testHeadlines = [new Headline{OriginalHeadline = "api test"}];
        var validKey = await service.Translate(testHeadlines);
        if (validKey.IsError)
        {
            logger.LogWarning("Failed to validate API key. Error: {Error}", validKey.FirstError.Description);
            return validKey.FirstError;
        }
        
        logger.LogDebug("API key validated successfully");
        return true;
    }

    internal async Task<ErrorOr<bool>> CheckScraperKey(IScrapingService service)
    {
        logger.LogInformation("Starting to check api key for scraping service");
        
        var testOutlet = new NewsOutlet
        {
            Website = new Uri("https://httpbin.io/anything"),
            NewsPassword = "{\n  \"links\": \"a @href\",\n  \"images\":\"img @src\"\n}",
            Name = "null",
            MainPagePassword = "{\n  \"links\": \"a @href\",\n  \"images\":\"img @src\"\n}",
            Themes = []
        };
        
        var validKey = await service.ScrapeWebsiteWithRetry(testOutlet);
        if (validKey.IsError)
        {
            logger.LogWarning("Failed to validate API key. Error: {Error}", validKey.FirstError.Description);
            return validKey.FirstError;
        }
        
        logger.LogDebug("API key validated successfully");
        return true;
    }
}