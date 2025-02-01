using dtr_nne.Domain.Entities.ScrapableEntities;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingResultProcessor;

public interface IScrapingResultProcessor
{
    public List<NewsArticle> ProcessResult<T>(string scrapeResult, T entity) where T : IScrapableEntity;
}