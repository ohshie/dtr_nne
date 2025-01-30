using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser;

internal class NewsParseHelper(ILogger<NewsParseHelper> logger, 
    IExternalServiceProvider serviceProvider, 
    IRepository<NewsOutlet> newsOutletRepository) : INewsParseHelper 
{
    public IScrapingService? RequestScraper()
    {
        try
        {
            return serviceProvider.Provide(ExternalServiceType.Scraper) as IScrapingService;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went wrong when attempting to fetch currently active existing Scraping Service: {ErrorStack}, {ErrorMessage}", e.Message, e.StackTrace);
            return null;
        }
    }

    public ITranslatorService? RequestTranslator()
    {
        try
        {
            return serviceProvider.Provide(ExternalServiceType.Translator) as ITranslatorService;
            
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went wrong when attempting to fetch currently active existing Translator Serivce: {ErrorStack}, {ErrorMessage}", e.Message, e.StackTrace);
            return null;
        }
    }
    
    public async Task<ErrorOr<List<NewsOutlet>>> RequestOutlets(NewsArticle? targetParse = null)
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
        
        return targetParse == null
            ? activeOutlets
            : FilterNewsOutlets(activeOutlets, targetParse);
    }

    private List<NewsOutlet> FilterNewsOutlets(List<NewsOutlet> outlets, NewsArticle filter)
    {
        var filteredOutlets = outlets.Where(no => no.Website.Host == filter.Website?.Host).ToList();
        logger.LogInformation("Matched article to news outlet: {OutletName}", filteredOutlets[0].Name);
        
        return filteredOutlets;
    }
}

internal interface INewsParseHelper
{
    public IScrapingService? RequestScraper();
    public ITranslatorService? RequestTranslator();
    public Task<ErrorOr<List<NewsOutlet>>> RequestOutlets(NewsArticle? targetParse = null);
}