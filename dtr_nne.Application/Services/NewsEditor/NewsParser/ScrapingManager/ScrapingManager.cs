using System.Runtime.CompilerServices;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager.MainPageScrapingResultProcessor;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;

public class ScrapingManager(ILogger<ScrapingManager> logger, 
    IMainPageScrapingResultProcessor mainPageScrapingResultProcessor) : IScrapingManager
{ 
    public async Task<ErrorOr<List<NewsArticle>>> BatchProcess<T>(List<T> entities, IScrapingService service)
    {
        try
        {
            var semaphore = new SemaphoreSlim(5);
            var tasks = await LoadSemaphore(entities, service, semaphore);

            var articles = (await Task.WhenAll(tasks))
                .SelectMany(na => na)
                .ToList();
            return articles;
        }
        catch (Exception e)
        {
            logger.LogError(
                "Unexpected error happened while trying to process incoming batch {EntityType}. {ExceptionMessage}, {ExceptionTrace}",
                typeof(T), e.Message, e.StackTrace);
            return Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(e.Message);
        }
    }

    internal async Task<List<Task<List<NewsArticle>>>> LoadSemaphore<T>(List<T> entities, 
        IScrapingService service, 
        SemaphoreSlim semaphore)
    {
        var tasks = new List<Task<List<NewsArticle>>>();

        foreach (var entity in entities)
        {
            await semaphore.WaitAsync();
            
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    return await ProcessEntity(entity, service);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        return tasks;
    }

    internal async Task<List<NewsArticle>> ProcessEntity<T>(T entity, IScrapingService service)
    {
        return entity switch
        {
            NewsOutlet outlet => await ScrapeMainPage(service, outlet),
            NewsArticle article => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }

    internal async Task<List<NewsArticle>> ScrapeMainPage(IScrapingService service, NewsOutlet outlet)
    {
        var scrapeResult = await service.ScrapeWebsiteWithRetry(outlet.Website, 
            outlet.MainPagePassword);
        if (scrapeResult.IsError)
        {
            return
            [
                new NewsArticle
                {
                    Error = scrapeResult.FirstError.Description,
                    NewsOutlet = outlet
                }
            ];
        }
                
        var newsArticles = mainPageScrapingResultProcessor.ProcessResult(scrapeResult.Value, outlet);
        return newsArticles;
    }

    internal async Task<NewsArticle> ScrapeNewsArticle(IScrapingService service, NewsArticle article)
    {
        var scrapeResult = await service.ScrapeWebsiteWithRetry(article.Uri!, 
            article.NewsOutlet!.NewsPassword);
        if (scrapeResult.IsError)
        {
            article.Error = scrapeResult.FirstError.Description;
        }
        article.ArticleContent.Body = scrapeResult.Value;

        return article;
    }
}