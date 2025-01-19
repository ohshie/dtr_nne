using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.ExternalServices;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.NewsEditor.NewsParser.NewsCollector;
using dtr_nne.Domain.Entities;
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
        if (RequestScraper() is not { } activeScrapingService)
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;
     
        if (RequestTranslator() is not { } activeTranslatorServiceService)
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;

        ErrorOr<List<NewsArticle>> workResult;
        
        var activeOutlets = await RequestOutlets();
        if (activeOutlets.IsError)
            return activeOutlets.FirstError;
        
        if (fullProcess)
        {
            workResult = await FullProcess(activeScrapingService, 
                activeTranslatorServiceService, activeOutlets.Value);
        }
        else
        {
            workResult = await newsCollector.Collect(activeScrapingService, 
                activeTranslatorServiceService, activeOutlets.Value);
        }

        if (workResult.IsError) 
            return workResult.FirstError;

        var newsArticleDtos = mapper.MassNewsArticleToDto(workResult.Value);
        return newsArticleDtos;
    }

    public async Task<ErrorOr<NewsArticleDto>> Execute(BaseNewsArticleDto articleDto)
    {
        var newsArticle = mapper.BaseNewsArticleDtoToNewsArticle(articleDto);

        var newsOutlets = await RequestOutlets(newsArticle);
        if (newsOutlets.IsError)
            return newsOutlets.FirstError;
        
        newsArticle.NewsOutlet = newsOutlets.Value.First();
        
        if (RequestScraper() is not { } activeScrapingService)
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;

        var processedArticle = await contentCollector.Collect(activeScrapingService, [newsArticle]);
        if (processedArticle.IsError)
            return processedArticle.FirstError;

        var processedArticleDto = mapper.NewsArticleToDto(processedArticle.Value.First());

        return processedArticleDto;
    }
    
    internal async Task<ErrorOr<List<NewsArticle>>> FullProcess(IScrapingService scrapingService, 
        ITranslatorService translatorService, List<NewsOutlet> activeOutlets)
    {
        var freshNewsList = await newsCollector.Collect(scrapingService, translatorService, activeOutlets);
        if (freshNewsList.IsError) return freshNewsList.FirstError;

        if (freshNewsList.Value.Count < 1)
        {
            return new List<NewsArticle>([]);
        }
        
        var processedArticles = await contentCollector.Collect(scrapingService, freshNewsList.Value);
        if (processedArticles.IsError) return processedArticles.FirstError;
        
        return processedArticles;
    }
    
    internal async Task<ErrorOr<List<NewsOutlet>>> RequestOutlets(NewsArticle? targetParse = null)
    {
        if (await newsOutletRepository.GetAll() is not { } allOutlets)
            return Errors.NewsOutlets.NotFoundInDb;
        
        var activeOutlets = allOutlets.Where(no => no.InUse).ToList();
        if (activeOutlets.Count < 1) 
            return Errors.NewsOutlets.NotFoundInDb;

        if (targetParse is null) return activeOutlets;

        var outlet = activeOutlets.FirstOrDefault(no => no.Website.Host == targetParse.Uri!.Host);
        if (outlet is null)
            return Errors.NewsOutlets.MatchFailed;
            
        return new List<NewsOutlet>([outlet]);
    }
        
    internal IScrapingService? RequestScraper()
    {
        try
        {
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