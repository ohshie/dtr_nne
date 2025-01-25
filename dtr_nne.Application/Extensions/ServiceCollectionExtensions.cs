using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;
using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.EntityManager;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Application.Services.NewsEditor.NewsParser;
using dtr_nne.Application.Services.NewsEditor.NewsParser.NewsCollector;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager.MainPageScrapingResultProcessor;
using dtr_nne.Application.Services.NewsEditor.NewsRewriter;
using dtr_nne.Domain.Entities.ManagedEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dtr_nne.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        #region ManagedEntities
        
        serviceCollection
            .AddTransient<IGetManagerEntity<OpenAiAssistantDto>,
                GetManagerEntity<OpenAiAssistant, OpenAiAssistantDto>>();
        serviceCollection
            .AddTransient<IGetManagerEntity<NewsOutletDto>,
                GetManagerEntity<NewsOutlet, NewsOutletDto>>();
        
        serviceCollection
            .AddTransient<IAddManagedEntity<OpenAiAssistantDto>,
                AddManagedEntity<OpenAiAssistant, OpenAiAssistantDto>>();
        serviceCollection
            .AddTransient<IAddManagedEntity<NewsOutletDto>,
                AddManagedEntity<NewsOutlet, NewsOutletDto>>();
        
        serviceCollection
            .AddTransient<IDeleteManagedEntity<OpenAiAssistantDto>,
                DeleteManagedEntity<OpenAiAssistant, OpenAiAssistantDto>>();
        serviceCollection
            .AddTransient<IDeleteManagedEntity<NewsOutletDto>,
                DeleteManagedEntity<NewsOutlet, NewsOutletDto>>();
        serviceCollection
            .AddTransient<IDeleteManagedEntity<BaseNewsOutletsDto>,
                DeleteManagedEntity<NewsOutlet, BaseNewsOutletsDto>>();

        serviceCollection
            .AddTransient<IUpdateManagedEntity<OpenAiAssistantDto>,
                UpdateManagedEntity<OpenAiAssistant, OpenAiAssistantDto>>();
        serviceCollection
            .AddTransient<IUpdateManagedEntity<NewsOutletDto>,
                UpdateManagedEntity<NewsOutlet, NewsOutletDto>>();
        
        serviceCollection.AddTransient<IManagedEntityHelper<OpenAiAssistant>, ManagedEntityHelper<OpenAiAssistant>>();
        serviceCollection.AddTransient<IManagedEntityHelper<NewsOutlet>, ManagedEntityHelper<NewsOutlet>>();
        serviceCollection.AddTransient<IManagedEntityMapper, ManagedEntityMapper>();
        
        #endregion

        #region ExternalServiceManager

        serviceCollection.AddTransient<IExternalServiceManagerHelper, ExternalServiceManagerHelper>();
        serviceCollection.AddTransient<IAddExternalService, AddExternalService>();
        serviceCollection.AddTransient<IGetExternalService, GetExternalService>();
        serviceCollection.AddTransient<IUpdateExternalService, UpdateExternalService>();
        serviceCollection.AddTransient<IDeleteExternalService, DeleteExternalService>();
        serviceCollection.AddTransient<IExternalServiceMapper, ExternalServiceMapper>();

        #endregion
        
        serviceCollection.AddTransient<INewsOutletMapper, NewsOutletMapper>();
        
        serviceCollection.AddTransient<IArticleMapper, ArticleMapper>();

        serviceCollection.AddTransient<IScrapingManager, ScrapingManager>();
        serviceCollection.AddTransient<IMainPageScrapingResultProcessor, MainPageScrapingResultProcessor>();
        serviceCollection.AddTransient<INewsCollector, NewsCollector>();
        serviceCollection.AddTransient<IContentCollector, ContentCollector>();
        serviceCollection.AddTransient<INewsParser, NewsParser>();

        serviceCollection.AddTransient<INewsRewriter, NewsRewriter>();
    }
}