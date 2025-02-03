using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.Repositories;

#pragma warning disable CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
internal class NewsArticleRepository(ILogger<NewsArticleRepository> logger, 
    IUnitOfWork<NneDbContext> unitOfWork) 
    : GenericRepository<NewsArticle, NneDbContext>(logger, unitOfWork), INewsArticleRepository
{
    private readonly IUnitOfWork<NneDbContext> _unitOfWork = unitOfWork;

    public async Task<List<NewsArticle>?> GetAllWithChildren()
    {
        try
        {
            var articles = await unitOfWork.Context.NewsArticles
                .Include(na => na.NewsOutlet)
                .Include(na => na.ArticleContent)
                .ThenInclude(ac => ac!.Headline)
                .Include(na => na.ArticleContent!.EditedArticle)
                .ToListAsync();

            return articles;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error produced while attempting to getting all saved articles with children");
            return null;
        }
    }

    public async Task<IEnumerable<NewsArticle>> GetLatestResults()
    {
        var articles = await _unitOfWork.Context.NewsArticles
            .Include(c => c.ArticleContent)
            .Include(c => c.ArticleContent)
            .ThenInclude(c => c!.Headline)
            .Where(na => na.ParseTime > DateTime.Now.AddDays(-2))
            .ToListAsync();

        return articles;
    }

    public bool RemoveArticleRange(List<NewsArticle> articles)
    {
        try
        {
            unitOfWork.Context.NewsArticles.RemoveRange(articles);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Deletion of {ArticleCount} news articles failed", articles.Count);
            return false;
        }
    }
}