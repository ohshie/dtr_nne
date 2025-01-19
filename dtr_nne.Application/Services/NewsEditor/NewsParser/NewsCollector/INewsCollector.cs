using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.NewsCollector;

public interface INewsCollector
{
    public Task<ErrorOr<List<NewsArticle>>> Collect(IScrapingService scraper, ITranslatorService translator, List<NewsOutlet> outlets);
}