using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser;

internal class BatchNewsParser(INewsParseProcessor newsParseProcessor, 
    IArticleParseProcessor articleParseProcessor,
    INewsParseHelper newsParseHelper,
    IRepository<NewsArticle> newsArticleRepository,
    IUnitOfWork<INneDbContext> unitOfWork) : IBatchNewsParser
{
    public async Task<ErrorOr<List<NewsArticle>>> ExecuteBatchParse(bool fullProcess = true)
    {
        var outlets = await newsParseHelper.RequestOutlets();
        if (outlets.IsError) return outlets.FirstError;

        if (newsParseHelper.RequestScraper() is not { } scraper)
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;
        
        if (newsParseHelper.RequestTranslator() is not {} translator)
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;

        var results = await ManageFlow(scraper, translator, outlets.Value, fullProcess);
        if (results.IsError)
        {
            return results.FirstError;
        }

        var processedArticles = await HeadlineTranslation(results.Value, translator);
        
        return processedArticles;
    }

    private async Task<ErrorOr<List<NewsArticle>>> ManageFlow(IScrapingService scraper,
        ITranslatorService translator, List<NewsOutlet> outlets, bool fullProcess)
    {
        var partialResults = await newsParseProcessor.Collect(scraper, translator, outlets);
        if (partialResults.IsError)
            return partialResults.FirstError;
        
        if (fullProcess)
        {
            partialResults = await articleParseProcessor.Collect(scraper, partialResults.Value);
            if (partialResults.IsError)
                return partialResults.FirstError;
        }
        
        newsArticleRepository.AttachRange(partialResults.Value);
        await newsArticleRepository.AddRange(partialResults.Value);
        await unitOfWork.Save();

        return partialResults;
    }

    private async Task<List<NewsArticle>> HeadlineTranslation(List<NewsArticle> articlesToBeTranslated, ITranslatorService translator)
    {
        var headlineTranslationResult = await translator.Translate(articlesToBeTranslated.Select(r => r.ArticleContent!.Headline!).ToList());
        if (headlineTranslationResult.IsError)
        {
            articlesToBeTranslated.ForEach(a => 
                a.ArticleContent!.Headline!.TranslatedHeadline = headlineTranslationResult.FirstError.Description);
        }
        else
        {
            
            var translationMap = headlineTranslationResult.Value
                .ToDictionary(h => h.Id, h => h);
    
            articlesToBeTranslated.ForEach(article => 
            {
                if (translationMap.TryGetValue(article.ArticleContent!.Headline!.Id, out var translated))
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