using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.ExternalServices;

public interface IExternalServiceFactory
{
    ILlmService CreateOpenAiService(ExternalService service);
    ITranslatorService CreateDeeplService(ExternalService service);
    IScrapingService CreateZenrowsService(ExternalService service);
}