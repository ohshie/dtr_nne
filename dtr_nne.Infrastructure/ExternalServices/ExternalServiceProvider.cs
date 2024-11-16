using dtr_nne.Application.ExternalServices;
using dtr_nne.Application.ExternalServices.LlmServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace dtr_nne.Infrastructure.ExternalServices;

public class ExternalServiceProvider(IServiceProvider serviceProvider, 
    IExternalServiceProviderRepository repository) : IExternalServiceProvider
{
    public async Task<IExternalService> GetService(ExternalServiceType type)
    {
        var requestedServices = await repository.GetByType(type) as List<ExternalService>;
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
                    var service = serviceProvider.GetRequiredService<IOpenAiService>();
                    return service;
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
}