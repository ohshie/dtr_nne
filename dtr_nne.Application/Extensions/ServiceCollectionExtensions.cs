using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;
using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.EntityManager;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Application.Services.NewsArticleManager;
using dtr_nne.Application.Services.NewsEditor.NewsParser;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.HeadlineTranslateProcessor;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingProcessor;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingResultProcessor;
using dtr_nne.Application.Services.NewsEditor.NewsRewriter;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dtr_nne.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        #region ManagedEntities
        
        serviceCollection
            .AddTransient<IGetManagedEntity<OpenAiAssistantDto>,
                GetManagedEntity<OpenAiAssistant, OpenAiAssistantDto>>();
        serviceCollection
            .AddTransient<IGetManagedEntity<NewsOutletDto>,
                GetManagedEntity<NewsOutlet, NewsOutletDto>>();
        
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
        
        serviceCollection
            .AddTransient<IGetManagedEntity<NewsArticleDto>,
                GetManagedEntity<NewsArticle, NewsArticleDto>>();
        
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

        #region Parsing
        
        serviceCollection.AddTransient<INewsParseManager, NewsParseManager>();
        serviceCollection.AddTransient<INewsParseHelper, NewsParseHelper>();
        serviceCollection.AddTransient<IBatchNewsParser, BatchNewsParser>();
        serviceCollection.AddTransient<INewsParser, NewsParser>();

        serviceCollection.AddTransient<INewsParseProcessor, NewsParseProcessor>();
        serviceCollection.AddTransient<IArticleParseProcessor, ArticleParseProcessor>();
        
        serviceCollection.AddTransient<IScrapingProcessor, ScrapingProcessor>();
        serviceCollection.AddTransient<IScrapingResultProcessor, ScrapingResultProcessor>();
        
        serviceCollection.AddTransient<IHeadlineTranslationProcessor, HeadlineTranslationProcessor>();

        #endregion

        serviceCollection.AddTransient<IDeleteNewsArticle, DeleteNewsArticle>();
        serviceCollection.AddTransient<IGetNewsArticles, GetNewsArticles>();
        
        serviceCollection.AddTransient<INewsOutletMapper, NewsOutletMapper>();
        serviceCollection.AddTransient<IArticleMapper, ArticleMapper>();
        
        serviceCollection.AddTransient<INewsRewriter, NewsRewriter>();
    }
}