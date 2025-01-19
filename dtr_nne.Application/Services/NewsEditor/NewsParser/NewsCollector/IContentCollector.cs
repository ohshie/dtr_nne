using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.NewsCollector;

public interface IContentCollector
{
    public Task<ErrorOr<List<NewsArticle>>> Collect(IScrapingService scraper, List<NewsArticle> newsList);
}