using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ArticleProcessor;

public class ArticleProcessor(ILogger<ArticleProcessor> logger, 
    IScrapingManager scrapingManager) : IArticleProcessor
{  
    public async Task<ErrorOr<List<NewsArticle>>> ProcessNews(List<NewsArticle> unprocessedArticles, 
        IScrapingService service)
    {
        logger.LogInformation("Starting to process incoming new news articles. Total amount: {AmountToBeProcessed}", unprocessedArticles.Count);

        var scrapedArticles = await scrapingManager.BatchProcess(unprocessedArticles, service);
        if (scrapedArticles.IsError)
        {
            return scrapedArticles.FirstError;
        }

        return scrapedArticles;
    }
}