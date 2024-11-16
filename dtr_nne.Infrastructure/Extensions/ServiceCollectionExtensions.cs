using dtr_nne.Application.ExternalServices;
using dtr_nne.Application.ExternalServices.LlmServices;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.ExternalServices;
using dtr_nne.Infrastructure.ExternalServices.LlmServices;
using dtr_nne.Infrastructure.ExternalServices.TranslatorServices;
using dtr_nne.Infrastructure.Repositories;
using dtr_nne.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dtr_nne.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        // Db
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        serviceCollection.AddDbContext<INneDbContext, NneDbContext>(s =>
        {
            s.UseSqlite(connectionString);
        });

        // Unit Of Work
        serviceCollection.AddScoped<IUnitOfWork<INneDbContext>, UnitOfWork<NneDbContext>>();
        serviceCollection.AddScoped<IUnitOfWork<NneDbContext>, UnitOfWork<NneDbContext>>();
        
        // Repositories
        serviceCollection.AddScoped<INewsOutletRepository, NewsOutletRepository>();
        serviceCollection.AddScoped<ITranslatorApiRepository, TranslatorApiRepository>();
        serviceCollection.AddScoped<IExternalServiceProviderRepository, ExternalServiceProviderRepository>();
        
        // Services
        serviceCollection.AddTransient<ITranslatorService, DeeplTranslator>();
        serviceCollection.AddTransient<IOpenAiService, OpenAiService>();
        
        // Providers
        serviceCollection.AddTransient<IExternalServiceProvider, ExternalServiceProvider>();
    }
}