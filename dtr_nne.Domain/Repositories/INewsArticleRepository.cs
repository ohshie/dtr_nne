using dtr_nne.Domain.Entities.ScrapableEntities;

namespace dtr_nne.Domain.Repositories;

public interface INewsArticleRepository : IRepository<NewsArticle>
{
    public Task<IEnumerable<NewsArticle>> GetLatestResults();
    bool RemoveArticleRange(List<NewsArticle> articles);
    Task<List<NewsArticle>?> GetAllWithChildren();
}