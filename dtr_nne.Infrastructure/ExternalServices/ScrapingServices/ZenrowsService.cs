using System.Web;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.ExternalServices;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.ExternalServices.ScrapingServices;

public class ZenrowsService(ILogger<ZenrowsService> logger, ExternalService service, IHttpClientFactory clientFactory) 
    : IZenrowsService 
{
    private readonly string _baseUri = "https://api.zenrows.com/v1/";
    private (Uri uri, string? cssExtractor, string? waitTimer, bool alwaysJs) _settings;
    public async Task<ErrorOr<string>> ScrapeWebsiteWithRetry<T>(T scrapingSettings, int maxRetries = 2)
    {
        var content = string.Empty;
        
        CreateSettingsContainer(scrapingSettings);
        
        for (var i = 0; i <= maxRetries; i++)
        {
            var requestUrl = BuildRequestString(service.ApiKey);
            
            var result = await ScrapeWebsite(requestUrl);
            if (!result.IsError)
            {   
                logger.LogInformation("Successfully processed URL: {Url}", _settings.uri.AbsoluteUri);
                content = result.Value;
                break;
            }

            if (i >= maxRetries)
            {
                return result.FirstError;
            }
            
            logger.LogError(
                "Failed to process URL: {Url} without JS Rendering, attempting to scrape it again with JS rendering",
                _settings.uri.AbsoluteUri);

            if (!_settings.alwaysJs)
            {
                _settings.alwaysJs = true;
            }
        }

        return content;
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

    private void CreateSettingsContainer<T>(T settingsContainer)
    {
        _settings = settingsContainer switch
        {
            NewsArticle na => (na.Uri!, na.NewsOutlet!.NewsPassword, na.NewsOutlet.WaitTimer,
                na.NewsOutlet.AlwaysJs),
            NewsOutlet no => (no.Website, "", no.WaitTimer, no.AlwaysJs),
            _ => _settings
        };
    }
    
    private string BuildRequestString(string apiKey)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query.Add("apikey", apiKey);

        query.Add("url", _settings.uri.AbsoluteUri);
    
        if (!string.IsNullOrEmpty(_settings.cssExtractor))
        {
            query.Add("css_extractor", _settings.cssExtractor);
        }
    
        if (!string.IsNullOrEmpty(_settings.waitTimer))
        {
            query.Add("wait", _settings.waitTimer);
        }
    
        if (_settings.alwaysJs)
        {
            query.Add("js_render", "true");
        }
    
        return $"{_baseUri}?{query}";
    }
}