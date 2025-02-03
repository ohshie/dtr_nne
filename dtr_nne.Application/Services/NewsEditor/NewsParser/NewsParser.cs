using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing;
using dtr_nne.Domain.Entities.ScrapableEntities;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser;

internal class NewsParser(INewsParseHelper newsParseHelper, 
    IArticleParseProcessor articleParseProcessor) : INewsParser
{
    public async Task<ErrorOr<NewsArticle>> ExecuteParse(NewsArticle newsArticle)
    {
        if (await newsParseHelper.RequestScraper() is not { } scraper)
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;

        if (await newsParseHelper.RequestTranslator() is not { } translator)
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;
        
        var outlets = await newsParseHelper.RequestOutlets(newsArticle);
        if (outlets.IsError) 
            return outlets.FirstError;
        
        newsArticle.NewsOutlet = outlets.Value[0];

        var parseResult = await articleParseProcessor.Collect(scraper, [newsArticle]);

        if (parseResult.IsError)
        {
            return parseResult.FirstError;
        }

        var headlineTranslationResult = await translator.Translate([parseResult.Value[0].ArticleContent!.Headline]);
        parseResult.Value[0].ArticleContent!.Headline.TranslatedHeadline = headlineTranslationResult.IsError
            ? headlineTranslationResult.FirstError.Description
            : headlineTranslationResult.Value[0].TranslatedHeadline;
        
        return parseResult.Value[0];
    }
}

public interface INewsParser
{
    public Task<ErrorOr<NewsArticle>> ExecuteParse(NewsArticle newsArticle);
}