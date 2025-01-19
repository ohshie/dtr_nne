using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.Repositories;

internal class NewsArticleRepository(ILogger<NewsArticleRepository> logger, 
    IUnitOfWork<NneDbContext> unitOfWork) 
    : GenericRepository<NewsArticle, NneDbContext>(logger, unitOfWork), INewsArticleRepository
{
    private readonly IUnitOfWork<NneDbContext> _unitOfWork = unitOfWork;
    
    public async Task<IEnumerable<NewsArticle>> GetSpecificAmount(int amount)
    {
        var articles = await _unitOfWork.Context.NewsArticles
            .OrderByDescending(na => na.Id)
            .Take(amount)
            .ToListAsync();

        return articles;
    }

    public async Task<IEnumerable<NewsArticle>> GetLatestResults()
    {
        var articles = await _unitOfWork.Context.NewsArticles
            .Where(na => na.ParseTime > DateTime.Now.AddDays(-2))
            .ToListAsync();

        return articles;
    }

    public async Task<bool> AddRange(List<NewsArticle> articles)
    {
        try
        {
            foreach (var article in articles)
            {
                if (article.NewsOutlet is not null)
                {
                    unitOfWork.Context.NewsOutlets.Entry(article.NewsOutlet!).State = EntityState.Unchanged;
                }
                
                await DbSet.AddAsync(article);
            }
            
            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Something went really wrong when trying to AddRange to Db {Exception}, \n {ExceptionTrace} \n {ExceptionInnerException}", 
                e.Message, 
                e.StackTrace, 
                e.InnerException?.Message ?? "No Inner Exception");
            throw;
        }
    }
}