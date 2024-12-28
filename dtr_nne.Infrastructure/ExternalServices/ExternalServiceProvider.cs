using dtr_nne.Application.ExternalServices;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace dtr_nne.Infrastructure.ExternalServices;

public class ExternalServiceProvider(IServiceProvider serviceProvider, 
    IExternalServiceProviderRepository repository) : IExternalServiceProvider
{
    public IExternalService GetExistingInUseService(ExternalServiceType type)
    {
        var requestedServices = repository.GetByType(type);
        if (requestedServices is null || requestedServices.Count < 1)
        {
            throw new InvalidOperationException($"No Service of type {Enum.GetName(typeof(ExternalServiceType), type)} is Found in Db");
        }

        if (!requestedServices.Any(s => s.InUse))
        {
            throw new InvalidOperationException($"No Enabled Service of {Enum.GetName(typeof(ExternalServiceType), type)} is Found in Db");
        }

        var inUseService = requestedServices.First(s => s.InUse);

        switch (type)
        {
            case ExternalServiceType.Llm:
                if (inUseService.ServiceName == Enum.GetName(typeof(LlmServiceType), type))
                {
                    return serviceProvider.GetRequiredService<IOpenAiService>();
                }
                break;
            case ExternalServiceType.Translator:
                if (inUseService.ServiceName == Enum.GetName(typeof(TranslatorType), type))
                {
                    return serviceProvider.GetRequiredService<ITranslatorService>();
                }
                break;
            case ExternalServiceType.Scraper:
                throw new NotImplementedException("Scraper service is not yet implemented");
        }
        
        throw new InvalidOperationException($"Unsupported service type {type}.");
    }

    public IExternalService ProvideService(ExternalServiceType type)
    {
        switch (type)
        {
            case ExternalServiceType.Llm:
                return serviceProvider.GetRequiredService<IOpenAiService>();
            case ExternalServiceType.Translator:
                return serviceProvider.GetRequiredService<ITranslatorService>();
            case ExternalServiceType.Scraper:
                throw new NotImplementedException("Scraper service is not yet implemented");
        }
        
        throw new InvalidOperationException($"Unsupported service type {type}.");
    }
}