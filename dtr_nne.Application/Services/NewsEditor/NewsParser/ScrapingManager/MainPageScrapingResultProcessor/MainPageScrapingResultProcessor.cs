using System.Web;
using dtr_nne.Domain.Entities;
using HtmlAgilityPack;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager.MainPageScrapingResultProcessor;

public class MainPageScrapingResultProcessor(ILogger<MainPageScrapingResultProcessor> logger) : IMainPageScrapingResultProcessor
{
    public List<NewsArticle> ProcessResult(string scrapeResult, NewsOutlet outlet)
    {
        var newsArticles = CreateNewsArticle(scrapeResult, outlet);
        return newsArticles;
    }

    internal List<NewsArticle> CreateNewsArticle(string html, NewsOutlet outlet)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode
            .SelectNodes(outlet.MainPagePassword);
        
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