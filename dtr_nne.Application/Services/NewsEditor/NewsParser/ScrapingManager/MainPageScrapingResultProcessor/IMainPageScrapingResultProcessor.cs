using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager.MainPageScrapingResultProcessor;

public interface IMainPageScrapingResultProcessor
{
    public List<NewsArticle> ProcessResult(string scrapeResult, NewsOutlet outlet);
}