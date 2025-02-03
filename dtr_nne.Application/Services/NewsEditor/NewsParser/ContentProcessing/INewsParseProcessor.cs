using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing;

public interface INewsParseProcessor
{
    public Task<ErrorOr<List<NewsArticle>>> Collect(IScrapingService scraper, ITranslatorService translator, List<NewsOutlet> outlets);
}