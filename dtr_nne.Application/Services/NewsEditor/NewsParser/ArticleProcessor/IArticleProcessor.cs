using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ArticleProcessor;

public interface IArticleProcessor
{
    public Task<ErrorOr<List<NewsArticle>>>
        ProcessNews(List<NewsArticle> unprocessedArticles, IScrapingService service);
}