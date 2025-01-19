using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.NewsCollector;

public class NewsCollector(ILogger<NewsCollector> logger, 
    IScrapingManager scrapingManager, 
    INewsArticleRepository newsArticleRepository,
    IUnitOfWork<INneDbContext> unitOfWork) : INewsCollector
{
    public async Task<ErrorOr<List<NewsArticle>>> Collect(IScrapingService scraper, 
        ITranslatorService translator, List<NewsOutlet> outlets)
    {
        logger.LogInformation("Starting news collection for {OutletCount} outlets", outlets.Count);
        
        var newArticlesList = await scrapingManager.BatchProcess(outlets, scraper);
        if (newArticlesList.IsError) {
            logger.LogError("Batch processing failed: {Error}", newArticlesList.FirstError);
            return newArticlesList.FirstError;
        }
        
        logger.LogInformation("Successfully scraped {ArticleCount} articles from {OutletCount} outlets", 
            newArticlesList.Value.Count, outlets.Count);
        
        var latestArticles = await FilterDuplicates(newArticlesList.Value);

        var finalResult = await TranslateHeadlines(translator, latestArticles);
        if (finalResult.IsError)
        {
            logger.LogError("Translation failed: {Error}", finalResult.FirstError.Description);
            return latestArticles
                .Select(article => 
                {
                    article.Error = finalResult.FirstError.Description;
                    return article;
                }).ToList();
        }
        
        logger.LogDebug("Starting database operations for {Count} articles", latestArticles.Count);
        await newsArticleRepository.AddRange(latestArticles);
        await unitOfWork.Save();

        return finalResult;
    }
    
    internal async Task<List<NewsArticle>> FilterDuplicates(List<NewsArticle> freshArticles)
    {
        logger.LogInformation("Starting to checking parsed news list for duplicates, " +
                              "currently there are {NewsCount} articles", 
            freshArticles.Count);

        List<NewsArticle> filteredList;
        
        if (await newsArticleRepository.GetLatestResults() is List<NewsArticle> currentRegisteredNews &&
            currentRegisteredNews.Count != 0)
        {
            filteredList = freshArticles
                .Where(fresh => currentRegisteredNews
                    .All(registered => registered.Uri != fresh.Uri)).ToList();
            
            logger.LogDebug("Removed {Count} duplicate articles", 
                freshArticles.Count - filteredList.Count);
        }
        else
        {
            logger.LogDebug("No existing articles found in database, keeping all fresh articles");
            filteredList = freshArticles;
        }
        
        logger.LogInformation("After filtering there are {NewsCount} left", filteredList.Count);
        return filteredList;
    }
    
    internal async Task<ErrorOr<List<NewsArticle>>> TranslateHeadlines(ITranslatorService translator, List<NewsArticle> articles)
    {
        logger.LogInformation("Starting headline translation for {Count} articles", articles.Count);
        
        var headlines = articles
            .Select(sa => sa.ArticleContent!.Headline)
            .ToList();
        
        var translatorResult = await translator.Translate(headlines);
        if (translatorResult.IsError)
        {
            logger.LogError("Translation service failed: {Error}", translatorResult.FirstError);
            return translatorResult.FirstError;
        }

        var finalResult = articles.Join(
            translatorResult.Value,
            article => article.ArticleContent!.Headline.OriginalHeadline,
            translation => translation.OriginalHeadline,
            (article, translation) =>
            {
                article.ArticleContent!.Headline = translation;
                return article;
            }
        ).ToList();
        
        var unmatched = articles.Count - finalResult.Count;
        if (unmatched > 0)
        {
            logger.LogWarning("Failed to match translations for {Count} articles", unmatched);
        }

        return finalResult;
    }
}