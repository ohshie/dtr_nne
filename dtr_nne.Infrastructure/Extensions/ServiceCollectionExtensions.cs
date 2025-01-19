using dtr_nne.Application.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.ExternalServices;
using dtr_nne.Infrastructure.Repositories;
using dtr_nne.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            s.UseSqlite(connectionString, builder => builder.MigrationsAssembly("dtr_nne.Infrastructure"))
                .ConfigureWarnings(builder => builder.Log(RelationalEventId.PendingModelChangesWarning));
        });

        // Unit Of Work
        serviceCollection.AddScoped<IUnitOfWork<INneDbContext>, UnitOfWork<NneDbContext>>();
        serviceCollection.AddScoped<IUnitOfWork<NneDbContext>, UnitOfWork<NneDbContext>>();
        
        // Repositories
        serviceCollection.AddScoped<INewsOutletRepository, NewsOutletRepository>();
        serviceCollection.AddScoped<IExternalServiceProviderRepository, ExternalServiceProviderRepository>();
        serviceCollection.AddScoped<IOpenAiAssistantRepository, OpenAiAssistantRepository>();
        serviceCollection.AddScoped<INewsArticleRepository, NewsArticleRepository>();
        
        // Providers
        serviceCollection.AddTransient<IExternalServiceProvider, ExternalServiceProvider>();
        serviceCollection.AddTransient<IExternalServiceFactory, ExternalServiceFactory>();
        
        serviceCollection.AddHttpClient();
    }
}