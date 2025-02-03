using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace dtr_nne.Application.Services.ExternalServices;

[SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded")]
internal class ExternalServiceManagerHelper(ILogger<ExternalServiceManagerHelper> logger, 
    IExternalServiceProvider serviceProvider, 
    IExternalServiceProviderRepository repository,
    IUnitOfWork<INneDbContext> unitOfWork) : IExternalServiceManagerHelper
{
    public async Task<ErrorOr<bool>> CheckKeyValidity(ExternalService incomingService)
    {
        if (await serviceProvider.Provide(incomingService.Type, incomingService.ApiKey) is not { } externalService)
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
                success = await CheckTranslatorKey((externalService as ITranslatorService)!);
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
    
    public async Task<ErrorOr<ExternalService>> FindRequiredExistingService(ExternalService serviceDto)
    {
        if (await repository.GetByType(serviceDto.Type) is not { } currentServices)
        {
            return Errors.ExternalServiceProvider.Service.NoSavedServiceFound;
        }

        if (currentServices.Find(s => s.ServiceName == serviceDto.ServiceName) is not { } serviceToUpdate)
        {
            return Errors.ExternalServiceProvider.Service.NoSavedServiceFound;
        }

        return serviceToUpdate;
    }

    public async Task<ErrorOr<bool>> PerformDataOperation(ExternalService service, string action)
    {
        switch (action)
        {
            case "update": 
                if (!repository.Update(service))
                {
                    logger.LogError("Failed to update External service in repository");
                    return Errors.DbErrors.UpdatingDbFailed;
                }
                break;
            case "delete":
                if (!repository.Remove(service))
                {
                    logger.LogError("Failed to remove External service in repository");
                    return Errors.DbErrors.RemovingFailed;
                }
                break;
            case "add":
                if (!await repository.Add(service))
                {
                    logger.LogError("Failed to add External service to repository");
                    return Errors.DbErrors.AddingToDbFailed;
                }
                break;
        }
        
        var success = await unitOfWork.Save();
        if (success) return success;
                
        logger.LogError("Failed to save API key in unit of work");
        return Errors.DbErrors.UnitOfWorkSaveFailed;
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
    
    internal async Task<ErrorOr<bool>> CheckTranslatorKey(ITranslatorService service)
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
            NewsPassword = "//div[contains(@class,'flex-shrink-0')]//a",
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

internal interface IExternalServiceManagerHelper
{
    public Task<ErrorOr<bool>> CheckKeyValidity(ExternalService incomingService);
    public Task<ErrorOr<ExternalService>> FindRequiredExistingService(ExternalService serviceDto);
    public Task<ErrorOr<bool>> PerformDataOperation(ExternalService service, string action);
}