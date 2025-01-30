using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingProcessor;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingResultProcessor;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing;

public class ArticleParseProcessor(ILogger<ArticleParseProcessor> logger, 
    IScrapingProcessor scrapingProcessor, IScrapingResultProcessor scrapingResultProcessor) : IArticleParseProcessor
{
    public async Task<ErrorOr<List<NewsArticle>>> Collect(IScrapingService scraper, List<NewsArticle> newsList)
    {
        logger.LogInformation("Starting to collect content from {ArticleCount} articles", newsList.Count);
        var scrapeResultList = await scrapingProcessor.BatchProcess(newsList, scraper);
        if (scrapeResultList.IsError)
        {
            logger.LogError("Encountered {ErrorDescription} collecting content from news articles", 
                scrapeResultList.FirstError.Description);
            return scrapeResultList.FirstError;
        }
        logger.LogInformation("Processed {ArticleCount} articles", scrapeResultList.Value.Count);
        var newsArticles = CreateNewsArticlesFromResults(scrapeResultList.Value);
        logger.LogInformation("Created {ArticleCount} full articles (including errors)", newsArticles.Count);
        
        logger.LogInformation("Successfully processed {ScrapedArticleCount}", scrapeResultList.Value.Count);
        return newsArticles;
    }

    private List<NewsArticle> CreateNewsArticlesFromResults(List<(NewsArticle, ErrorOr<string>)> articleResults)
    {
        List<NewsArticle> articles = [];
        foreach (var article in articleResults)
        {
            if (article.Item2.IsError)
            {
                article.Item1.Error = article.Item2.FirstError.Description;
                articles.Add(article.Item1);
                continue;
            }

            articles.AddRange(scrapingResultProcessor.ProcessResult(article.Item2.Value, article.Item1));
        }

        return articles;
    }
}