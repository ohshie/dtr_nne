using System.Diagnostics.CodeAnalysis;
using dtr_nne.Application.ExternalServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.ExternalServices;

public class ExternalServiceProvider(ILogger<ExternalServiceProvider> logger, 
    IExternalServiceProviderRepository repository, 
    IExternalServiceFactory serviceFactory) : IExternalServiceProvider
{
    [Experimental("OPENAI001")]
    public IExternalService Provide(ExternalServiceType type, string apiKey = "")
    {
        logger.LogInformation("Providing external service of type {Type}", type);
        
        var service = ConstructService(apiKey, type);
        
        switch (type)
        {
            case ExternalServiceType.Llm:
                    logger.LogDebug("Creating OpenAI service");
                    return serviceFactory.CreateOpenAiService(service);
            case ExternalServiceType.Translator:
                    logger.LogDebug("Creating translator service");
                    return serviceFactory.CreateDeeplService(service);
            case ExternalServiceType.Scraper:
                logger.LogWarning("Scraper service requested but not implemented");
                throw new NotImplementedException("Scraper service is not yet implemented");
        }
        
        logger.LogError("Unsupported service type: {Type}", type);
        throw new InvalidOperationException($"Unsupported service type {type}.");
    }   

    internal ExternalService ConstructService(string apiKey, ExternalServiceType type)
    {
        logger.LogDebug("Constructing service for type {Type}", type);
        if (string.IsNullOrEmpty(apiKey))
        {
            var requestedServices = repository.GetByType(type); 
            if (requestedServices is null || requestedServices.Count < 1)
            {
                logger.LogError("No services found for type {Type}", type);
                throw new InvalidOperationException($"No Service of type {Enum.GetName(typeof(ExternalServiceType), type)} is Found in Db");
            }

            if (!requestedServices.Any(s => s.InUse))
            {
                logger.LogError("No enabled services found for type {Type}", type);
                throw new InvalidOperationException($"No Enabled Service of {Enum.GetName(typeof(ExternalServiceType), type)} is Found in Db");
            }
        
            var service = requestedServices.First(s => s.InUse);
            logger.LogInformation("Using existing service {ServiceName} of type {Type}", service.ServiceName, type);
            return service;
        }

        logger.LogInformation("Creating new service of type {Type} with provided API key", type);
        return new ExternalService
        {
            ApiKey = apiKey,
            InUse = true,
            Type = type
        };
    }
}