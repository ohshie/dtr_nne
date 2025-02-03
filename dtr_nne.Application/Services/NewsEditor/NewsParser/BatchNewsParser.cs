using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser;

internal class BatchNewsParser(INewsParseProcessor newsParseProcessor, 
    IArticleParseProcessor articleParseProcessor,
    INewsParseHelper newsParseHelper) : IBatchNewsParser
{
    public async Task<ErrorOr<List<NewsArticle>>> ExecuteBatchParse(bool fullProcess = true)
    {
        var outlets = await newsParseHelper.RequestOutlets();
        if (outlets.IsError) return outlets.FirstError;

        if (await newsParseHelper.RequestScraper() is not { } scraper)
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;
        
        if (await newsParseHelper.RequestTranslator() is not {} translator)
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;

        var results = fullProcess
            ? await FullProcess(scraper, translator, outlets.Value)
            : await newsParseProcessor.Collect(scraper, translator, outlets.Value);
        if (results.IsError)
        {
            return results.FirstError;
        }

        var processedArticles = await HeadlineTranslation(results.Value, translator);
        
        return processedArticles;
    }

    private async Task<ErrorOr<List<NewsArticle>>> FullProcess(IScrapingService scraper,
        ITranslatorService translator, List<NewsOutlet> outlets)
    {
        var collectedArticles = await newsParseProcessor.Collect(scraper, translator, outlets);
        if (collectedArticles.IsError)
            return collectedArticles.FirstError;

        var completeArticles = await articleParseProcessor.Collect(scraper, collectedArticles.Value);

        return completeArticles;
    }

    private async Task<List<NewsArticle>> HeadlineTranslation(List<NewsArticle> articlesToBeTranslated, ITranslatorService translator)
    {
        var headlineTranslationResult = await translator.Translate(articlesToBeTranslated.Select(r => r.ArticleContent!.Headline).ToList());
        if (headlineTranslationResult.IsError)
        {
            articlesToBeTranslated.ForEach(a => 
                a.ArticleContent!.Headline.TranslatedHeadline = headlineTranslationResult.FirstError.Description);
        }
        else
        {
            var translationMap = headlineTranslationResult.Value
                .ToDictionary(h => h.OriginalHeadline, h => h);
    
            articlesToBeTranslated.ForEach(article => 
            {
                if (translationMap.TryGetValue(article.ArticleContent!.Headline.OriginalHeadline, out var translated))
                {
                    article.ArticleContent.Headline = translated;
                }
            });
        }

        return articlesToBeTranslated;
    }
    
}

public interface IBatchNewsParser
{
    public Task<ErrorOr<List<NewsArticle>>> ExecuteBatchParse(bool fullProcess = true);
}