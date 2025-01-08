using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.NewsSearcher;

public class NewsSearcher(ILogger<NewsSearcher> logger, 
    IScrapingManager scrapingManager, 
    INewsOutletRepository newsOutletRepository,
    INewsArticleRepository newsArticleRepository,
    IUnitOfWork<INneDbContext> unitOfWork) : INewsSearcher
{
    internal async Task<ErrorOr<List<NewsArticle>>> CollectNews(IScrapingService service, string? cutOffDate = null)
    {
        logger.LogInformation("Starting to collect all updates from saved outlets");
        
        if (await newsOutletRepository.GetAll() is not { } outlets)
        {
            logger.LogWarning("there are currently no saved news outlets");
            return Errors.NewsOutlets.NotFoundInDb;
        }

        var news = await scrapingManager.ProcessMainPages(service, (outlets as List<NewsOutlet>)!);
        if (news.IsError)
        {
            return news.FirstError;
        }

        var filteredNews = await FilterDuplicates(news.Value);
        if (filteredNews.Count is 0)
        {
            logger.LogWarning("No new news articles left after filtering. Returning null");
            return Errors.NewsAticles.NoNewNewsArticles;
        }

        await newsArticleRepository.AddRange(filteredNews);
        await unitOfWork.Save();

        return filteredNews;
    }

    internal async Task<List<NewsArticle>> FilterDuplicates(List<NewsArticle> incomingArticles)
    {
        logger.LogInformation("Starting to checking parsed news list or duplicates, currently there are {NewsCount} articles", incomingArticles.Count);
        
        List<NewsArticle> filteredList = [];
        if (await newsArticleRepository.GetSpecificAmount(incomingArticles.Count) is List<NewsArticle> currentArticles && currentArticles.Count > 0)
        {
            filteredList = incomingArticles
                .Where(incoming => currentArticles
                    .All(current => current.Uri != incoming.Uri))
                .ToList();
        }
        else
        {
            filteredList = incomingArticles;
        }
        
        logger.LogInformation("After filtering there are {NewsCount} left", filteredList.Count);
        return filteredList;
    }
}