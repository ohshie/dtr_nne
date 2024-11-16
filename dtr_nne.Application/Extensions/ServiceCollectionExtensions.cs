using dtr_nne.Application.ExternalServices.TranslatorServices;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.NewsOutletServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dtr_nne.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddTransient<INewsOutletService, NewsOutletService>();
        serviceCollection.AddTransient<INewsOutletMapper, NewsOutletMapper>();

        serviceCollection.AddTransient<ITranslatorApiKeyService, TranslatorApiKeyService>();
        serviceCollection.AddTransient<IApiKeyMapper, ApiKeyMapper>();
    }
}