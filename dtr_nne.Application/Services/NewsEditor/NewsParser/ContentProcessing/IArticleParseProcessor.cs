using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing;

public interface IArticleParseProcessor
{
    public Task<ErrorOr<List<NewsArticle>>> Collect(IScrapingService scraper, List<NewsArticle> newsList);
}