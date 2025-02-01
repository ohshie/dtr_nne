using System.Text;
using System.Web;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.ExternalServices.ScrapingServices;

public class ZenrowsService(ILogger<ZenrowsService> logger, ExternalService service, IHttpClientFactory clientFactory) 
    : IZenrowsService 
{
    private readonly string _baseUri = "https://api.zenrows.com/v1/";
    public async Task<ErrorOr<string>> ScrapeWebsiteWithRetry<T>(T entity, int maxRetries = 2)
    where T : IScrapableEntity
    {
        var content = string.Empty;
        var requestUrl = CreateRequestString(entity);
        
        for (var i = 0; i <= maxRetries; i++)
        {
            var result = await ScrapeWebsite(requestUrl);
            if (!result.IsError)
            {   
                logger.LogInformation("Successfully processed URL: {Url}", entity.Website.AbsoluteUri);
                content = result.Value;
                break;
            }

            if (i >= maxRetries)
            {
                return result.FirstError;
            }

            if (requestUrl.Contains("js_render")) continue;
            
            logger.LogWarning(
                "Failed to process URL: {Url} without JS Rendering, attempting to scrape it again with JS rendering",
                entity.Website.AbsoluteUri);
            requestUrl = new StringBuilder(requestUrl).Append("?js_render=true").ToString();
        }

        return content;
    }

    private string CreateRequestString<T>(T entity)
    where T : IScrapableEntity
    {
        return entity switch
        {
            NewsOutlet outlet => BuildRequestString(outlet, service.ApiKey),
            NewsArticle article => BuildRequestString(article, service.ApiKey),
            _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, null)
        };
    }
    
    private async Task<ErrorOr<string>> ScrapeWebsite(string requestUrl)
    {
        using (var client = clientFactory.CreateClient())
        {
            try
            {
                var response = await client.GetAsync(requestUrl);
                
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Something went wrong trying to scrape {OutletUrl} {Exception}\n {StackTrace}", requestUrl, e.Message, e.StackTrace);
                return Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(e.Message);
            }
        }
    }
    
    private string BuildRequestString(NewsOutlet newsOutlet, string apiKey)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query.Add("apikey", apiKey);

        query.Add("url", newsOutlet.Website.AbsoluteUri);
        
        if (!string.IsNullOrEmpty(newsOutlet.WaitTimer))
        {
            query.Add("wait", newsOutlet.WaitTimer);
        }
    
        if (newsOutlet.AlwaysJs)
        {
            query.Add("js_render", "true");
        }
    
        return $"{_baseUri}?{query}";
    }
    
    internal string BuildRequestString(NewsArticle newsArticle, string apiKey)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query.Add("apikey", apiKey);
        query.Add("premium_proxy", "true");

        query.Add("url", newsArticle.Website!.AbsoluteUri);
        
        if (!string.IsNullOrEmpty(newsArticle.NewsOutlet!.WaitTimer))
        {
            query.Add("wait", newsArticle.NewsOutlet.WaitTimer);
        }
    
        if (newsArticle.NewsOutlet.AlwaysJs)
        {
            query.Add("js_render", "true");
        }
    
        return $"{_baseUri}?{query}";
    }
}