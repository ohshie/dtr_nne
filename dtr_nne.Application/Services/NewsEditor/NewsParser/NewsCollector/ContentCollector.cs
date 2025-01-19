using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.NewsCollector;

public class ContentCollector(ILogger<ContentCollector> logger, IScrapingManager scrapingManager) : IContentCollector
{
    public async Task<ErrorOr<List<NewsArticle>>> Collect(IScrapingService scraper, List<NewsArticle> newsList)
    {
        logger.LogInformation("Starting to collect content from {ArticleCount} articles", newsList.Count);
        var scrapedArticles = await scrapingManager.BatchProcess(newsList, scraper);
        if (scrapedArticles.IsError)
        {
            logger.LogError("Encountered {ErrorDescription} collecting content from news articles", 
                scrapedArticles.FirstError.Description);
            return scrapedArticles.FirstError;
        }
        
        logger.LogInformation("Successfully processed {ScrapedArticleCount}", scrapedArticles.Value.Count);
        return scrapedArticles;
    }
}