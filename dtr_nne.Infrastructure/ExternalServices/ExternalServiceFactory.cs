using System.Diagnostics.CodeAnalysis;
using dtr_nne.Application.ExternalServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using dtr_nne.Infrastructure.ExternalServices.LlmServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.ExternalServices;

public class ExternalServiceFactory(IServiceProvider provider) : IExternalServiceFactory
{
    [Experimental("OPENAI001")]
    public ILlmService CreateOpenAiService(ExternalService? service = null)
    {
        var logger = provider.GetRequiredService<ILogger<OpenAiService>>();
        var repository = provider.GetRequiredService<IOpenAiAssistantRepository>();

        return new OpenAiService(logger: logger, 
            repository: repository,
            service: service);
    }

    public ITranslatorService CreateTranslatorService(ExternalService service)
    {
        throw new NotImplementedException();
    }
}