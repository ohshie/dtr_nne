using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingResultProcessor;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Systems.Services.InternalServices.TestNewsParser.TestContentProcessing;

public class TestScrapingResultProcessor
{
    public TestScrapingResultProcessor()
    {
        _sut = new ScrapingResultProcessor(Substitute.For<ILogger<ScrapingResultProcessor>>());
    }

    private readonly ScrapingResultProcessor _sut;

    private readonly string _newsOutletHtml =
        "<html><body><a href='/article1'>Article 1</a><a href='/article2'>Article 2</a></body></html>";

    private readonly string _newsArticleHtml =
        "<html><body><h1 class='header'>Test Headline</h1><img src='https://example.com/image1.jpg'/><p class='copyright'>Copyright 2023</p><p class='body'>Test body content.</p><p class='source'>Test Source</p></body></html>";
    
    private static readonly NewsOutlet TestNewsOutlet = new()
    {
        Name = "Test Outlet",
        MainPagePassword = "//a",
        NewsPassword = "//body.//h1.//img.//p[contains(@class, 'copyright')].//p[contains(@class, 'body')].//p[contains(@class, 'source')]",
        Website = new Uri("https://example.com"),
        Themes = ["science"]
    };
    
    private readonly NewsArticle _testNewsArticle = new()
    {
        Website = new Uri("https://example.com/article1"),
        NewsOutlet = TestNewsOutlet,
        ArticleContent = new()
        {
            Headline = new Headline()
        }
    };
    
    [Fact]
    public void ProcessResult_ForNewsOutlet_ReturnsListOfNewsArticles()
    {
        // Arrange

        // Act
        var result = _sut.ProcessResult(_newsOutletHtml, TestNewsOutlet);

        // Assert
        result.Should().HaveCount(2);
        result[0].Website.Should().Be(new Uri("https://example.com/article1"));
        result[0].ArticleContent!.Headline.OriginalHeadline.Should().Be("Article 1");
        result[1].Website.Should().Be(new Uri("https://example.com/article2"));
        result[1].ArticleContent!.Headline.OriginalHeadline.Should().Be("Article 2");
    }
    
    [Fact]
    public void ProcessResult_ForNewsArticle_ReturnsFullArticle()
    {
        // Arrange
      
        // Act
        var result = _sut.ProcessResult(_newsArticleHtml, _testNewsArticle);

        // Assert
        result.Should().HaveCount(1);
        result[0].ArticleContent!.Headline.OriginalHeadline.Should().Be("Test Headline");
        result[0].ArticleContent!.Images.Should().ContainSingle().Which.Should().Be(new Uri("https://example.com/image1.jpg"));
        result[0].ArticleContent!.Copyright.Should().ContainSingle().Which.Should().Be("Copyright 2023");
        result[0].ArticleContent!.Body.Should().Be("Test body content.");
        result[0].ArticleContent!.Source.Should().Be("Test Source");
    }
    
    [Fact]
    public void CreateUri_WithAbsoluteUrl_ReturnsUri()
    {
        // Arrange
        var url = "https://example.com/article1";
       
        // Act
        var result = _sut.CreateUri(url, TestNewsOutlet);

        // Assert
        result.Should().Be(new Uri("https://example.com/article1"));
    }

    [Fact]
    public void CreateUri_WithRelativeUrl_ReturnsCombinedUri()
    {
        // Arrange
        var url = "/article1";

        // Act
        var result = _sut.CreateUri(url, TestNewsOutlet);

        // Assert
        result.Should().Be(new Uri("https://example.com/article1"));
    }
    
    [Fact]
    public void InitializeArticleStructure_ReturnsInitializedArticle()
    {
        // Arrange

        // Act
        var result = _sut.InitializeArticleStructure(_testNewsArticle);

        // Assert
        result.NewsOutlet.Should().Be(_testNewsArticle.NewsOutlet);
        result.Website.Should().Be(_testNewsArticle.Website);
        result.ParseTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        result.ArticleContent.Should().NotBeNull();
        result.ArticleContent!.Headline.Should().NotBeNull();
        result.ArticleContent.Images.Should().BeEmpty();
        result.ArticleContent.Copyright.Should().BeEmpty();
        result.ArticleContent.Body.Should().BeEmpty();
    }
    
    [Fact]
    public void ValidateAndParseSelectors_WithValidNewsPassword_ReturnsSelectors()
    {
        // Arrange

        // Act
        var result = _sut.ValidateAndParseSelectors(TestNewsOutlet.NewsPassword, 6);

        // Assert
        result.Should().HaveCount(6);
        result[0].Should().Be("//body");
        result[1].Should().Be("//h1");
        result[2].Should().Be("//img");
        result[3].Should().Be("//p[contains(@class, 'copyright')]");
        result[4].Should().Be("//p[contains(@class, 'body')]");
        result[5].Should().Be("//p[contains(@class, 'source')]");
    }
    
    [Fact]
    public void ValidateAndParseSelectors_WithInvalidNewsPassword_ThrowsException()
    {
        // Arrange
        var newsPassword = "body.header";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.ValidateAndParseSelectors(newsPassword, 6));
    }
    
    [Fact]
    public void ExtractHeadline_WithValidHeaderNode_SetsHeadline()
    {
        // Arrange
        var html = "<html><body><h1 class='header'>Test Headline</h1></body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var mainNode = doc.DocumentNode.SelectSingleNode("//body");

        // Act
        _sut.ExtractHeadline(mainNode, "//h1", _testNewsArticle);

        // Assert
        _testNewsArticle.ArticleContent!.Headline.OriginalHeadline.Should().Be("Test Headline");
    }
    
    [Fact]
    public void ExtractHeadline_WithInvalidHeaderNode_SetsErrorInheadline()
    {
        // Arrange
        var html = "<html><body></body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var mainNode = doc.DocumentNode.SelectSingleNode("//body");

        // Act
        _sut.ExtractHeadline(mainNode, "//h1", _testNewsArticle);

        // Assert
        _testNewsArticle.ArticleContent!.Headline.OriginalHeadline.Should().Be("Header node not found using selector: //h1");
    }
    
    [Fact]
    public void ExtractImages_WithValidImageNodes_AddsImages()
    {
        // Arrange
        var html = "<html><body><img src='http://example.com/image1.jpg'/><img src='http://example.com/image2.jpg'/></body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var mainNode = doc.DocumentNode.SelectSingleNode("//body");

        // Act
        _sut.ExtractImages(mainNode!, "//img", _testNewsArticle);

        // Assert
        _testNewsArticle.ArticleContent!.Images.Should().HaveCount(2);
        _testNewsArticle.ArticleContent.Images[0].Should().Be(new Uri("http://example.com/image1.jpg"));
        _testNewsArticle.ArticleContent.Images[1].Should().Be(new Uri("http://example.com/image2.jpg"));
    }
    
    [Fact]
    public void ExtractCopyrights_WithValidCopyrightNodes_AddsCopyrights()
    {
        // Arrange
        var html = "<html><body><p class='copyright'>Copyright 2023</p><p class='copyright'>All rights reserved</p></body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var mainNode = doc.DocumentNode.SelectSingleNode("//body");

        // Act
        _sut.ExtractCopyrights(mainNode!, "//p", _testNewsArticle);

        // Assert
        _testNewsArticle.ArticleContent!.Copyright.Should().HaveCount(2);
        _testNewsArticle.ArticleContent.Copyright[0].Should().Be("Copyright 2023");
        _testNewsArticle.ArticleContent.Copyright[1].Should().Be("All rights reserved");
    }
    
    [Fact]
    public void ExtractBodyContent_WithValidBodyNodes_SetsBodyContent()
    {
        // Arrange
        var html = "<html><body><p class='body'>Test body content.</p><p class='body'>More content.</p></body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var mainNode = doc.DocumentNode.SelectSingleNode("//body");

        // Act
        _sut.ExtractBodyContent(mainNode!, "//p", _testNewsArticle);

        // Assert
        _testNewsArticle.ArticleContent!.Body.Should().Be("Test body content.\n\nMore content.");
    }
    
    [Fact]
    public void ExtractSource_WithValidSourceNode_SetsSource()
    {
        // Arrange
        var html = "<html><body><p class='source'>Test Source</p></body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var mainNode = doc.DocumentNode.SelectSingleNode("//body");


        // Act
        _sut.ExtractSource(mainNode!, "//p", _testNewsArticle);

        // Assert
        _testNewsArticle.ArticleContent!.Source.Should().Be("Test Source");
    }
}