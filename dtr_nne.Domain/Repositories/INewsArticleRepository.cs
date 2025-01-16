using dtr_nne.Domain.Entities;

namespace dtr_nne.Domain.Repositories;

public interface INewsArticleRepository : IRepository<NewsArticle>
{
    public Task<IEnumerable<NewsArticle>> GetSpecificAmount(int amount);
    public Task<bool> AddRange(List<NewsArticle> articles);
}