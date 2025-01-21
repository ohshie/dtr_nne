using System.Runtime.CompilerServices;
using System.Text.Json;
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
        logger.LogInformation("Starting batch process for {Count} entities of type {EntityType}", 
            entities.Count, typeof(T).Name);
        
        try
        {
            var semaphore = new SemaphoreSlim(5);
            var tasks = await LoadSemaphore(entities, service, semaphore);

            logger.LogInformation("Waiting for all scraping tasks to complete");
            var articles = (await Task.WhenAll(tasks))
                .SelectMany(na => na)
                .ToList();
            
            var errorCount = articles.Count(a => !string.IsNullOrEmpty(a.Error));
            logger.LogInformation("Batch processing completed. Total articles: {Total}, Successful: {Success}, Failed: {Failed}", 
                articles.Count, articles.Count - errorCount, errorCount);
            
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

        logger.LogDebug("Finished loading {Count} tasks into semaphore queue", tasks.Count);
        return tasks;
    }

    internal async Task<List<NewsArticle>> ProcessEntity<T>(T entity, IScrapingService service)
    {
        logger.LogDebug("Processing entity of type {EntityType}", typeof(T).Name);
        
        return entity switch
        {
            NewsOutlet outlet => await ScrapeMainPage(service, outlet),
            NewsArticle article => await ScrapeNewsArticle(service, article),
            _ => throw new NotImplementedException($"Processing not implemented for type {typeof(T).Name}")
        };
    }

    internal async Task<List<NewsArticle>> ScrapeMainPage(IScrapingService service, NewsOutlet outlet)
    {
        logger.LogInformation("Starting main page scrape for outlet: {OutletName}, URL: {Url}", 
            outlet.Name, outlet.Website);
        
        var scrapeResult = await service.ScrapeWebsiteWithRetry(outlet);
        if (scrapeResult.IsError)
        {
            logger.LogError("Failed to scrape main page for outlet {OutletName}: {Error}", 
                outlet.Name, scrapeResult.FirstError.Description);
            
            return
            [
                new NewsArticle
                {
                    Error = scrapeResult.FirstError.Description,
                    NewsOutlet = outlet
                }
            ];
        }
        
        logger.LogDebug("Successfully scraped main page for {OutletName}, processing results", outlet.Name);
        var newsArticles = mainPageScrapingResultProcessor.ProcessResult(scrapeResult.Value, outlet);
        
        logger.LogInformation("Processed {Count} articles from main page of {OutletName}", 
            newsArticles.Count, outlet.Name);
        return newsArticles;
    }

    internal async Task<List<NewsArticle>> ScrapeNewsArticle(IScrapingService service, NewsArticle article)
    {
        logger.LogInformation("Starting article scrape for URL: {Url}, Outlet: {OutletName}", 
            article.Uri, article.NewsOutlet?.Name);
        
        var scrapeResult = await service.ScrapeWebsiteWithRetry(article);
        if (scrapeResult.IsError)
        {
            logger.LogError("Failed to scrape article {Url}: {Error}", 
                article.Uri, scrapeResult.FirstError.Description);
            
            article.Error = scrapeResult.FirstError.Description;
            return [article];
        }

        try
        {
            logger.LogDebug("Attempting to deserialize article content for {Url}", article.Uri);
            article.ArticleContent = JsonSerializer.Deserialize<ArticleContent>(scrapeResult.Value);
            logger.LogInformation("Successfully scraped and processed article: {Url}", article.Uri);
        }
        catch (Exception e)
        {
            logger.LogError("JSON deserialization failed for article {Url}", article.Uri);
            article.Error = Errors.NewsArticles.JsonSerializationError(e.Message).Description;
        }
        
        return [article];
    }
}