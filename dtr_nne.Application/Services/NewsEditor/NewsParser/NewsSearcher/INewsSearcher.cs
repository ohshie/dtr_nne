using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.NewsSearcher;

public interface INewsSearcher
{
    public Task<ErrorOr<List<NewsArticle>>> CollectNews(IScrapingService service, string? cutOffDate = null);
}