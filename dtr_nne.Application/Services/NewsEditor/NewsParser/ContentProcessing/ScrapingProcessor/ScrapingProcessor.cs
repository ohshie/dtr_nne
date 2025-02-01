using System.Runtime.CompilerServices;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;

[assembly: InternalsVisibleTo("Tests")]
namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingProcessor;

public class ScrapingProcessor(ILogger<ScrapingProcessor> logger) : IScrapingProcessor
{ 
    public async Task<ErrorOr<List<(T, ErrorOr<string>)>>> BatchProcess<T>(List<T> entities, IScrapingService service)
    where T : IScrapableEntity
    {
        logger.LogInformation("Starting batch process for {Count} entities of type {EntityType}", 
            entities.Count, typeof(T).Name);
        
        try
        {
            var semaphore = new SemaphoreSlim(5);
            var tasks = await ScrapePagesWithSemaphore(entities, service, semaphore);

            logger.LogInformation("Waiting for all scraping tasks to complete");
            var scrapingResults = (await Task.WhenAll(tasks))
                .ToList();
            
            var errorCount = scrapingResults.Count(a => a.Item2.IsError);
            logger.LogInformation("Batch processing completed. Total articles: {Total}, Successful: {Success}, Failed: {Failed}", 
                scrapingResults.Count, scrapingResults.Count - errorCount, errorCount);
            
            return scrapingResults;
        }
        catch (Exception e)
        {
            logger.LogError(e, 
                "Unexpected error happened while trying to process incoming batch {EntityType}. {ExceptionMessage}, {ExceptionTrace}",
                typeof(T), e.Message, e.StackTrace);
            return Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(e.Message);
        }
    }

    private async Task<List<Task<(T, ErrorOr<string>)>>> ScrapePagesWithSemaphore<T>(List<T> entities, 
        IScrapingService service, 
        SemaphoreSlim semaphore)
    where T : IScrapableEntity
    {
        var tasks = new List<Task<(T, ErrorOr<string>)>>();

        foreach (var entity in entities)
        {
            tasks.Add(ProcessEntityAsync(entity, service, semaphore));
        }

        logger.LogDebug("Loaded {Count} tasks into semaphore queue", tasks.Count);
        return tasks;
    }

    private async Task<(T, ErrorOr<string>)> ProcessEntityAsync<T>(T entity, IScrapingService service, SemaphoreSlim semaphore) 
        where T : IScrapableEntity
    {
        await semaphore.WaitAsync();
        try
        {
            return await ScrapePage(entity, service);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<(T, ErrorOr<string>)> ScrapePage<T>(T entity, IScrapingService service) 
    where T : IScrapableEntity
    {
        logger.LogInformation("Starting to scrape webpage: {PageUrl}", entity.Website.AbsoluteUri);
        
        var scrapeResult = await service.ScrapeWebsiteWithRetry(entity);
        logger.LogInformation("{PageUrl} processed, success: {ResultIsError}", entity.Website.AbsoluteUri, !scrapeResult.IsError);
        return (entity, scrapeResult);
    }
}