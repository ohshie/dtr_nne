using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;

public class ScrapingManager : IScrapingManager
{
    public Task<ErrorOr<List<NewsArticle>>> ProcessMainPages(IScrapingService service, List<NewsOutlet> outlets)
    {
        throw new NotImplementedException();
    }

    public Task<ErrorOr<List<NewsArticle>>> ProcessArticlePages()
    {
        throw new NotImplementedException();
    }
}