using System.Text.Json;
using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager.MainPageScrapingResultProcessor;

public class MainPageScrapingResultProcessor(ILogger<MainPageScrapingResultProcessor> logger) : IMainPageScrapingResultProcessor
{
    public List<NewsArticle> ProcessResult(string scrapeResult, NewsOutlet outlet)
    {
        var articleUrls = JsonDocument.Parse(scrapeResult).RootElement.GetProperty("links").EnumerateArray();

        var newsArticles = articleUrls.Select(url => CreateNewsArticle(url, outlet)).ToList();
        return newsArticles;
    }

    internal NewsArticle CreateNewsArticle(JsonElement unprocessedUrl, NewsOutlet outlet)
    {
        var url = CreateUri(unprocessedUrl.GetString()!, outlet);
        
        var newsArticle = new NewsArticle
        {
            Uri = url,
            NewsOutlet = outlet,
            ParseTime = DateTime.Now,
            Themes = outlet.Themes
        };

        return newsArticle;
    }
    
    internal Uri CreateUri(string url, NewsOutlet newsOutlet)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme.StartsWith("http"))
        {
            return uri;
        }

        var cleanedUrl = url.TrimStart('/');
        var baseUri = newsOutlet.Website.Host;
        var combinedUrl = new Uri(baseUri+cleanedUrl);

        logger.LogInformation("Created combined URL: {CombinedUrl}", combinedUrl);
        return combinedUrl;
    }
}