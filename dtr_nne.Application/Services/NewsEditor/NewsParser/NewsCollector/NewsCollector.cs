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
        var newArticlesList = await scrapingManager.BatchProcess(outlets, scraper);
        if (newArticlesList.IsError) return newArticlesList.FirstError;
        
        var latestArticles = await FilterDuplicates(newArticlesList.Value);

        var finalResult = await TranslateHeadlines(translator, latestArticles);
        if (finalResult.IsError)
        {
            return latestArticles
                .Select(article => 
                {
                    article.Error = finalResult.FirstError.Description;
                    return article;
                }).ToList();
        }
        
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
        }
        else
        {
            filteredList = freshArticles;
        }
        
        logger.LogInformation("After filtering there are {NewsCount} left", filteredList.Count);
        return filteredList;
    }
    
    internal async Task<ErrorOr<List<NewsArticle>>> TranslateHeadlines(ITranslatorService translator, List<NewsArticle> articles)
    {
        var headlines = articles
            .Select(sa => sa.ArticleContent!.Headline)
            .ToList();
        
        var translatorResult = await translator.Translate(headlines);
        if (translatorResult.IsError)
            return translatorResult.FirstError;

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

        return finalResult;
    }
}