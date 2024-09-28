using dtr_nne.Application.Mapper;
using dtr_nne.Application.NewsOutletServices;
using dtr_nne.Application.TranslatorServices;
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
    }
}