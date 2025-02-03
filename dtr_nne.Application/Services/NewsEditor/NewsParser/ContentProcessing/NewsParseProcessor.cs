using System.Runtime.CompilerServices;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingProcessor;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingResultProcessor;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;

[assembly:InternalsVisibleTo("Tests")]
namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing;

public class NewsParseProcessor(ILogger<NewsParseProcessor> logger, 
    IScrapingProcessor scrapingProcessor, IScrapingResultProcessor scrapingResultProcessor,
    INewsArticleRepository newsArticleRepository) 
    : INewsParseProcessor
{
    public async Task<ErrorOr<List<NewsArticle>>> Collect(IScrapingService scraper, 
        ITranslatorService translator, List<NewsOutlet> outlets)
    {
        logger.LogInformation("Starting news collection for {OutletCount} outlets", outlets.Count);
        
        var scrapeResultList = await scrapingProcessor.BatchProcess(outlets, scraper);
        if (scrapeResultList.IsError) {
            logger.LogError("Batch processing completely failed: {Error}", scrapeResultList.FirstError.Description);
            return scrapeResultList.FirstError;
        }
        
        logger.LogInformation("Processed {OutletCount} outlets", scrapeResultList.Value.Count);
        var newsArticles = CreateArticlesFromResults(scrapeResultList.Value);
        logger.LogInformation("Collected {ArticleCount} articles (including errors)", newsArticles.Count);
        
        var latestArticles = await FilterExistingArticles(newsArticles);
        logger.LogInformation("Filtered to {FilteredCount} new articles", latestArticles.Count);

        return latestArticles;
    }

    private List<NewsArticle> CreateArticlesFromResults(List<(NewsOutlet, ErrorOr<string>)> outletResults)
    {
        var newsArticles = outletResults
            .SelectMany(scrapeResult =>
            {
                if (scrapeResult.Item2.IsError)
                {
                    logger.LogWarning("Scraping failed for {Outlet}: {Error}",
                        scrapeResult.Item1.Name, scrapeResult.Item2.FirstError.Description);
                    return new List<NewsArticle>
                    {
                        new()
                        {
                            NewsOutlet = scrapeResult.Item1,
                            Error = scrapeResult.Item2.FirstError.Description
                        }
                    };
                }

                var processedArticles = scrapingResultProcessor.ProcessResult(
                    scrapeResult.Item2.Value, scrapeResult.Item1);
                return processedArticles;
            })
            .ToList();

        return newsArticles;
    }
    
    private async Task<List<NewsArticle>> FilterExistingArticles(List<NewsArticle> freshArticles)
    {
        logger.LogInformation("Checking for existing articles to filter duplicates");

        var currentRegisteredNews = await newsArticleRepository.GetLatestResults();
        var existingWebsites = new HashSet<Uri>(currentRegisteredNews.Select(a => a.Website!));
        var filteredList = freshArticles
            .Where(fresh => !existingWebsites.Contains(fresh.Website!))
            .ToList();
        
        logger.LogInformation("Removed {DuplicateCount} duplicates",
            freshArticles.Count - filteredList.Count);
        
        return filteredList;
    }
}