using dtr_nne.Application.ExternalServices;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.NewsEditor.NewsRewriter;
using dtr_nne.Application.Services.NewsOutletServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dtr_nne.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddTransient<INewsOutletServiceHelper, NewsOutletServiceHelper>();
        serviceCollection.AddTransient<IGetNewsOutletService, GetNewsOutletService>();
        serviceCollection.AddTransient<IAddNewsOutletService, AddNewsOutletService>();
        serviceCollection.AddTransient<IUpdateNewsOutletService, UpdateNewsOutletService>();
        serviceCollection.AddTransient<IDeleteNewsOutletService, DeleteNewsOutletService>();
        
        serviceCollection.AddTransient<INewsOutletMapper, NewsOutletMapper>();
        serviceCollection.AddTransient<IExternalServiceMapper, ExternalServiceMapper>();
        serviceCollection.AddTransient<IArticleMapper, ArticleMapper>();

        serviceCollection.AddTransient<IExternalServiceManager, ExternalServiceManager>();

        serviceCollection.AddTransient<INewsRewriter, NewsRewriter>();
    }
}