using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using HtmlAgilityPack;

[assembly:InternalsVisibleTo("Tests")]
namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingResultProcessor;

public class ScrapingResultProcessor(ILogger<ScrapingResultProcessor> logger) : IScrapingResultProcessor
{
    public List<NewsArticle> ProcessResult<T>(string scrapeResult, T entity) 
        where T : IScrapableEntity
    {
        return entity switch
        {
            NewsOutlet no => CreateNewsArticle(scrapeResult, no),
            NewsArticle na => CreateNewsArticle(scrapeResult, na),
            _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, null)
        };
    }
    
    private List<NewsArticle> CreateNewsArticle(string html, NewsOutlet outlet)
    {
        logger.LogInformation("Starting to process scrape result for outlet: {OutletName}", outlet.Name);
        
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
            Website = CreateUri(a.GetAttributeValue("href", ""), outlet),
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
        
        logger.LogInformation("Successfully processed {Count} articles from {OutletName}", 
            articles.Count,
            outlet.Name);
        
        return articles;
    }
    
    private List<NewsArticle> CreateNewsArticle(string html, NewsArticle article)
    {
        const int requiredSelectorCount = 6;
    
        try
        {
            logger.LogInformation("Starting to process scrape result for article: {ArticleName}", article.Website);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var newArticle = InitializeArticleStructure(article);
            var selectors = ValidateAndParseSelectors(newArticle.NewsOutlet?.NewsPassword, requiredSelectorCount);

            var htmlMainNode = doc.DocumentNode.SelectSingleNode(selectors[0]);
            if (htmlMainNode == null)
            {
                logger.LogWarning("Main content node not found using selector: {Selector}", selectors[0]);
                newArticle.Error = $"Main content node not found using selector: {selectors[0]}";
                return [newArticle];
            }

            ExtractHeadline(htmlMainNode, selectors[1], newArticle);
            ExtractImages(htmlMainNode, selectors[2], newArticle);
            ExtractCopyrights(htmlMainNode, selectors[3], newArticle);
            ExtractBodyContent(htmlMainNode, selectors[4], newArticle);
            ExtractSource(htmlMainNode, selectors[5], newArticle);

            return [newArticle];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process article: {Website}", article.Website);
            article.Error = ex.Message;
            return [article];
        }
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
        var baseUri = newsOutlet.Website.AbsoluteUri;
        var combinedUrl = new Uri(baseUri+cleanedUrl);

        logger.LogInformation("Created combined URL: {CombinedUrl} from base: {BaseUri} and path: {Path}", 
            combinedUrl, baseUri, url);
        return combinedUrl;
    }
    
    internal NewsArticle InitializeArticleStructure(NewsArticle template)
    {
        return new NewsArticle
        {
            NewsOutlet = template.NewsOutlet,
            Website = template.Website,
            ParseTime = DateTime.Now,
            ArticleContent = new ArticleContent
            {
                Headline = new Headline(),
                Images = new List<Uri>(),
                Copyright = new List<string>(),
                Body = string.Empty
            }
        };
    }
    
    internal string[] ValidateAndParseSelectors(string? newsPassword, int requiredCount)
    {
        if (string.IsNullOrWhiteSpace(newsPassword))
            throw new ArgumentException("NewsPassword is missing or empty");

        var selectors = newsPassword.Split('.');
        if (selectors.Length < requiredCount)
            throw new ArgumentException($"NewsPassword requires {requiredCount} selector components separated by periods");

        return selectors;
    }
    
    internal void ExtractHeadline(HtmlNode mainNode, string headerSelector, NewsArticle article)
    {
        var headerNode = mainNode.SelectSingleNode($".{headerSelector}");
        if (headerNode == null)
        {
            logger.LogWarning("Header node not found using selector: {Selector}", headerSelector);
            article.ArticleContent!.Headline!.OriginalHeadline =
                $"Header node not found using selector: {headerSelector}";
            return;
        }

        if (string.IsNullOrEmpty(article.ArticleContent!.Headline!.OriginalHeadline))
        {
            article.ArticleContent.Headline.OriginalHeadline = HtmlEntity.DeEntitize(headerNode.InnerText).Trim();
        }
    }
    
    internal void ExtractImages(HtmlNode mainNode, string imageSelector, NewsArticle article)
    {
        var imageNodes = mainNode.SelectNodes($".{imageSelector}") ?? Enumerable.Empty<HtmlNode>();
        foreach (var imageNode in imageNodes)
        {
            var src = imageNode.GetAttributeValue("src", null);
            if (Uri.TryCreate(src, UriKind.Absolute, out var uri))
            {
                article.ArticleContent!.Images.Add(uri);
            }
            else
            {
                logger.LogWarning("Invalid or missing image source: {Source}", src);
            }
        }
    }
    
    internal void ExtractCopyrights(HtmlNode mainNode, string copyrightSelector, NewsArticle article)
    {
        var copyrightNodes = mainNode.SelectNodes($".{copyrightSelector}") ?? Enumerable.Empty<HtmlNode>();
        article.ArticleContent!.Copyright.AddRange(
            copyrightNodes.Select(n => HtmlEntity.DeEntitize(n.InnerText).Trim())
                .Where(text => !string.IsNullOrEmpty(text))
        );
    }
    
    internal void ExtractBodyContent(HtmlNode mainNode, string bodySelector, NewsArticle article)
    {
        var bodyNodes = mainNode.SelectNodes($".{bodySelector}") ?? Enumerable.Empty<HtmlNode>();
        var bodyBuilder = new StringBuilder();
    
        foreach (var bodyNode in bodyNodes)
        {
            var cleanedText = HtmlEntity.DeEntitize(bodyNode.InnerText).Trim();
        
            if (string.IsNullOrEmpty(cleanedText)) continue;
            
            bodyBuilder.AppendLine(cleanedText);
            bodyBuilder.AppendLine();
        }

        article.ArticleContent!.Body = bodyBuilder.ToString().TrimEnd('\r', '\n');
    }
    
    internal  void ExtractSource(HtmlNode mainNode, string sourceSelector, NewsArticle article)
    {
        if (string.IsNullOrWhiteSpace(sourceSelector)) return;

        var sourceNode = mainNode.SelectSingleNode($".{sourceSelector}");
        if (sourceNode != null)
        {
            article.ArticleContent!.Source = HtmlEntity.DeEntitize(sourceNode.InnerText).Trim();
        }
    }
}