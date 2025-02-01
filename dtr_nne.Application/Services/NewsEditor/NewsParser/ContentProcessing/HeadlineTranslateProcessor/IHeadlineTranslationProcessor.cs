using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.HeadlineTranslateProcessor;

public interface IHeadlineTranslationProcessor
{
    public Task<ErrorOr<List<NewsArticle>>> Translate(ITranslatorService translator, List<NewsArticle> articles);
}