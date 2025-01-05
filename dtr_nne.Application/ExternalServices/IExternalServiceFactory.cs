using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.ExternalServices;

public interface IExternalServiceFactory
{
    ILlmService CreateOpenAiService(ExternalService? service);
    ITranslatorService CreateDeeplService(ExternalService service);
    IScrapingService CreateZenrowsService(ExternalService service);
}