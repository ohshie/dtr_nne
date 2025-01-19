using System.Web;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.ExternalServices.ScrapingServices;

public class ZenrowsService(ILogger<ZenrowsService> logger, ExternalService service, IHttpClientFactory clientFactory) : IScrapingService
{
    private readonly string _baseUri = "https://api.zenrows.com/v1/";
    
    public async Task<ErrorOr<string>> ScrapeWebsiteWithRetry(Uri uri, string cssSelector, bool alwaysJs = false, int maxRetries = 2)
    {
        var content = string.Empty;
        
        for (var i = 0; i <= maxRetries; i++)
        {
            var requestUrl = BuildRequestString(uri, service.ApiKey, cssSelector, alwaysJs);
            
            var result = await ScrapeWebsite(requestUrl);
            if (!result.IsError)
            {
                logger.LogInformation("Successfully processed URL: {Url}", uri.AbsoluteUri);
                content = result.Value;
                break;
            }

            if (i >= maxRetries)
            {
                return result.FirstError;
            }
            
            logger.LogError(
                "Failed to process URL: {Url} without JS Rendering, attempting to scrape it again with JS rendering",
                uri);

            if (!alwaysJs)
            {
                alwaysJs = true;
            }
        }

        return content;
    }
    
    internal async Task<ErrorOr<string>> ScrapeWebsite(string requestUrl)
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
                logger.LogError("Something went wrong trying to scrape {OutletUrl} {Exception}\n {StackTrace}", requestUrl, e.Message, e.StackTrace);
                return Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(e.Message);
            }
        }
    }
    
    internal string BuildRequestString(Uri requestUri, string apiKey, string cssSelector, bool useJs)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query.Add("apikey", apiKey);
        query.Add("url", requestUri.AbsoluteUri);
        if (!string.IsNullOrEmpty(cssSelector))
        {
            query.Add("css_extractor", cssSelector);
        }
        
        if (useJs)
        {
            query.Add("js_render", "true");
        }
        
        return $"{_baseUri}?{query}";
    }
}