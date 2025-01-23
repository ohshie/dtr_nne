using System.Web;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using HtmlAgilityPack;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager.MainPageScrapingResultProcessor;

public class MainPageScrapingResultProcessor(ILogger<MainPageScrapingResultProcessor> logger) : IMainPageScrapingResultProcessor
{
    public List<NewsArticle> ProcessResult(string scrapeResult, NewsOutlet outlet)
    {
        logger.LogInformation("Starting to process scrape result for outlet: {OutletName}", outlet.Name);
        
        var newsArticles = CreateNewsArticle(scrapeResult, outlet);
        logger.LogInformation("Successfully processed {Count} articles from {OutletName}", 
            newsArticles.Count,
            outlet.Name);
        return newsArticles;
    }

    internal List<NewsArticle> CreateNewsArticle(string html, NewsOutlet outlet)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode
            .SelectNodes(outlet.MainPagePassword);
        
        logger.LogDebug("Found {Count} article nodes for outlet {OutletName}", 
            nodes.Count, outlet.Name);
        
        var articles = nodes
            .Select(a => 
                new NewsArticle
        {
            Uri = CreateUri(a.GetAttributeValue("href", ""), outlet),
            ArticleContent = new ArticleContent
            {
                Headline = new Headline
                {
                    OriginalHeadline = HttpUtility.HtmlDecode(a.InnerText.Trim())
                }
            },
                
            NewsOutlet = outlet, 
            Themes = outlet.Themes,
            ParseTime = DateTime.Now
        }).ToList();
        
        return articles;
    }
    
    internal Uri CreateUri(string url, NewsOutlet newsOutlet)
    {
        logger.LogDebug("Creating URI from URL: {Url} for outlet: {OutletName}", 
            url, newsOutlet.Name);
        
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme.StartsWith("http"))
        {
            logger.LogDebug("URL is already absolute: {Url}", url);
            return uri;
        }

        var cleanedUrl = url.TrimStart('/');
        var baseUri = newsOutlet.Website.Host;
        var combinedUrl = new Uri(baseUri+cleanedUrl);

        logger.LogInformation("Created combined URL: {CombinedUrl} from base: {BaseUri} and path: {Path}", 
            combinedUrl, baseUri, cleanedUrl);
        return combinedUrl;
    }
}