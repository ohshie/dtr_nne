using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.NewsCollector;

public class ContentCollector(ILogger<ContentCollector> logger, IScrapingManager scrapingManager) : IContentCollector
{
    public async Task<ErrorOr<List<NewsArticle>>> Collect(IScrapingService scraper, List<NewsArticle> newsList)
    {
        var scrapedArticles = await scrapingManager.BatchProcess(newsList, scraper);
        if (scrapedArticles.IsError) return scrapedArticles.FirstError;

        return scrapedArticles;
    }
}