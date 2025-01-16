using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;

public interface IScrapingManager
{
    internal Task<ErrorOr<List<NewsArticle>>> BatchProcess<T>(List<T> entities, IScrapingService service);
}