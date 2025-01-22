using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Application.Services.NewsEditor.NewsParser.NewsCollector;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser;

public class NewsParser(ILogger<NewsParser> logger, 
    IExternalServiceProvider serviceProvider, 
    IArticleMapper mapper,
    INewsOutletRepository newsOutletRepository,
    IContentCollector contentCollector,
    INewsCollector newsCollector) : INewsParser
{
    public async Task<ErrorOr<List<NewsArticleDto>>> ExecuteBatchParse(bool fullProcess = true, string cutOffTime = "")
    {
        logger.LogInformation("Starting batch parse operation. FullProcess: {FullProcess}, CutOffTime: {CutOffTime}", 
            fullProcess, cutOffTime);

        if (RequestScraper() is not { } activeScrapingService)
        {
            logger.LogError("No active scraping service found");
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;
        }
        
        if (RequestTranslator() is not { } activeTranslatorServiceService)
        {
            logger.LogError("No active translator service found");
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;
        }
        
        
        
        var activeOutlets = await RequestOutlets();
        if (activeOutlets.IsError)
        {
            logger.LogError("Failed to retrieve active news outlets: {Error}", activeOutlets.FirstError.Description);
            return activeOutlets.FirstError;
        }
        
        logger.LogInformation("Found {ActiveOutletsCount} active news outlets", activeOutlets.Value.Count);
        
        ErrorOr<List<NewsArticle>> workResult;
        if (fullProcess)
        {
            logger.LogInformation("Starting full processing workflow");
            workResult = await FullProcess(activeScrapingService, 
                activeTranslatorServiceService, activeOutlets.Value);
        }
        else
        {
            logger.LogInformation("Starting collection-only workflow");
            workResult = await newsCollector.Collect(activeScrapingService, 
                activeTranslatorServiceService, activeOutlets.Value);
        }

        if (workResult.IsError)
        {
            logger.LogError("Work process failed: {WorkResultError}", workResult.FirstError);
            return workResult.FirstError;
        }
        
        logger.LogInformation("Successfully processed {Count} articles", workResult.Value.Count);
        var newsArticleDtos = mapper.MassNewsArticleToDto(workResult.Value);
        return newsArticleDtos;
    }

    public async Task<ErrorOr<NewsArticleDto>> Execute(BaseNewsArticleDto articleDto)
    {
        logger.LogInformation("Starting single article processing for URL: {Url}", articleDto.Uri);
        var newsArticle = mapper.BaseNewsArticleDtoToNewsArticle(articleDto);

        var newsOutlets = await RequestOutlets(newsArticle);
        if (newsOutlets.IsError)
        {
            logger.LogError("Failed to find matching news outlet for {Url}: {Error}", 
                articleDto.Uri, newsOutlets.FirstError.Description);
            return newsOutlets.FirstError;
        }
        
        newsArticle.NewsOutlet = newsOutlets.Value.First();
        logger.LogInformation("Matched article to news outlet: {OutletName}", newsArticle.NewsOutlet.Name);
        
        if (RequestScraper() is not { } activeScrapingService)
        {
            logger.LogError("No active scraping service found");
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;
        }

        var processedArticle = await contentCollector.Collect(activeScrapingService, [newsArticle]);
        if (processedArticle.IsError)
        {
            logger.LogError("Content collection failed: {Error}", processedArticle.FirstError.Description);
            return processedArticle.FirstError;
        }

        var processedArticleDto = mapper.NewsArticleToDto(processedArticle.Value.First());
        logger.LogInformation("Successfully processed article with title: {Url}", processedArticleDto.Uri);
        return processedArticleDto;
    }
    
    internal async Task<ErrorOr<List<NewsArticle>>> FullProcess(IScrapingService scrapingService, 
        ITranslatorService translatorService, List<NewsOutlet> activeOutlets)
    {
        logger.LogInformation("Starting full process for {Count} outlets", activeOutlets.Count);
        var freshNewsList = await newsCollector.Collect(scrapingService, translatorService, activeOutlets);
        if (freshNewsList.IsError)
        {
            logger.LogError("News collection failed: {Error}", freshNewsList.FirstError.Description);
            return freshNewsList.FirstError;
        }

        if (freshNewsList.Value.Count < 1)
        {
            logger.LogInformation("No new articles found to process");
            return new List<NewsArticle>([]);
        }
        
        logger.LogInformation("Collected {Count} new articles, starting content processing", freshNewsList.Value.Count);
        var processedArticles = await contentCollector.Collect(scrapingService, freshNewsList.Value);
        if (processedArticles.IsError)
        {
            logger.LogError("Content processing failed: {Error}", processedArticles.FirstError.Description);
            return processedArticles.FirstError;
        }
        
        logger.LogInformation("Successfully processed {Count} articles", processedArticles.Value.Count);
        return processedArticles;
    }
    
    internal async Task<ErrorOr<List<NewsOutlet>>> RequestOutlets(NewsArticle? targetParse = null)
    {
        logger.LogDebug("Requesting news outlets");
        
        if (await newsOutletRepository.GetAll() is not { } allOutlets)
        {
            logger.LogError("No news outlets found in database");
            return Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet));
        }
        
        var activeOutlets = allOutlets.Where(no => no.InUse).ToList();
        if (activeOutlets.Count < 1)
        {
            logger.LogError("No active news outlets found");
            return Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet));
        }

        if (targetParse is null)
        {
            logger.LogInformation("Found {Count} active news outlets", activeOutlets.Count);
            return activeOutlets;
        }

        var outlet = activeOutlets.FirstOrDefault(no => no.Website.Host == targetParse.Uri!.Host);
        if (outlet is null)
        {
            logger.LogError("No matching outlet found for host: {Host}", targetParse.Uri!.Host);
            return Errors.ManagedEntities.NewsOutlets.MatchFailed;
        }
        
        logger.LogInformation("Found matching outlet: {OutletName} for host: {Host}", 
            outlet.Name, targetParse.Uri!.Host);
        return new List<NewsOutlet>([outlet]);
    }
        
    internal IScrapingService? RequestScraper()
    {
        try
        {
            logger.LogDebug("Requesting scraping service");
            var service = serviceProvider.Provide(ExternalServiceType.Scraper) as IScrapingService;
            return service;
        }
        catch (Exception e)
        {
            logger.LogError("Something went wrong when attempting to fetch currently active existing Scraping Service: {ErrorStack}, {ErrorMessage}", e.Message, e.StackTrace);
            return null;
        }
    }

    internal ITranslatorService? RequestTranslator()
    {
        try
        {
            logger.LogDebug("Requesting translator service");
            var service = serviceProvider.Provide(ExternalServiceType.Translator) as ITranslatorService;
            return service;
        }
        catch (Exception e)
        {
            logger.LogError("Something went wrong when attempting to fetch currently active existing Translator Serivce: {ErrorStack}, {ErrorMessage}", e.Message, e.StackTrace);
            return null;
        }
    }
}