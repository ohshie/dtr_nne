using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.HeadlineTranslateProcessor;

public class HeadlineTranslationProcessor(ILogger<HeadlineTranslationProcessor> logger) : IHeadlineTranslationProcessor
{
    public async Task<ErrorOr<List<NewsArticle>>> Translate(ITranslatorService translator, List<NewsArticle> articles)
    {
        logger.LogInformation("Starting headline translation for {Count} articles", articles.Count);
        
        var headlines = articles
            .Select(sa => sa.ArticleContent!.Headline)
            .ToList();
        
        var translatorResult = await translator.Translate(headlines!);
        if (translatorResult.IsError)
        {
            logger.LogError("Translation service failed: {Error}", translatorResult.FirstError);
            return translatorResult.FirstError;
        }

        var finalResult = articles.Select(article => {
            var matchingTranslation = translatorResult.Value
                .FirstOrDefault(t => 
                    t.OriginalHeadline == article.ArticleContent?.Headline!.OriginalHeadline);
    
            if (matchingTranslation != null)
            {
                article.ArticleContent!.Headline = matchingTranslation;
            }
    
            return article;
        }).ToList();
        
        var unmatched = articles.Count - finalResult.Count;
        if (unmatched > 0)
        {
            logger.LogWarning("Failed to match translations for {Count} articles", unmatched);
        }

        return finalResult;
    }
}