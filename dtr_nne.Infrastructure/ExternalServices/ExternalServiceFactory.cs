using System.Diagnostics.CodeAnalysis;
using dtr_nne.Application.ExternalServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using dtr_nne.Infrastructure.ExternalServices.LlmServices;
using dtr_nne.Infrastructure.ExternalServices.ScrapingServices;
using dtr_nne.Infrastructure.ExternalServices.TranslatorServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.ExternalServices;

public class ExternalServiceFactory(IServiceProvider provider) : IExternalServiceFactory
{
    [Experimental("OPENAI001")]
    public ILlmService CreateOpenAiService(ExternalService service)
    {
        var logger = provider.GetRequiredService<ILogger<OpenAiService>>();
        var repository = provider.GetRequiredService<IOpenAiAssistantRepository>();

        return new OpenAiService(logger: logger, 
            repository: repository,
            service: service);
    }

    public ITranslatorService CreateDeeplService(ExternalService service)
    {
        var logger = provider.GetRequiredService<ILogger<DeeplTranslator>>();

        return new DeeplTranslator(logger: logger, service: service);
    }

    public IScrapingService CreateZenrowsService(ExternalService service)
    {
        var logger = provider.GetRequiredService<ILogger<ZenrowsService>>();
        var httpClient = provider.GetRequiredService<IHttpClientFactory>();

        return new ZenrowsService(logger, service, httpClient);
    }
}