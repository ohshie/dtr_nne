using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingProcessor;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingResultProcessor;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;
using ErrorOr;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tests.Fixtures.NewsArticleFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsParser.TestContentProcessing;

public class TestArticleParseProcessor
{
    public TestArticleParseProcessor()
    {
        _scrapingProcessor = Substitute.For<IScrapingProcessor>();
        _scrapingResultProcessor = Substitute.For<IScrapingResultProcessor>();
        _scrapingService = Substitute.For<IScrapingService>();

        BasicSetup();
        
        _sut = new ArticleParseProcessor(Substitute.For<ILogger<ArticleParseProcessor>>(), _scrapingProcessor, _scrapingResultProcessor);
    }

    private readonly ArticleParseProcessor _sut;
    private readonly IScrapingProcessor _scrapingProcessor;
    private readonly IScrapingResultProcessor _scrapingResultProcessor;
    private readonly IScrapingService _scrapingService;
    
    private List<NewsArticle> _testNewsArticles = NewsArticleFixtureBase.Articles[0];
    private readonly List<(NewsArticle, ErrorOr<string>)> _testProcessResult =
    [
        new ValueTuple<NewsArticle, ErrorOr<string>>()
        {
            Item1 = NewsArticleFixtureBase.Articles[0][0],
            Item2 = "scraper result"
        }
    ];

    private void BasicSetup()
    {
        _scrapingProcessor.BatchProcess(_testNewsArticles, _scrapingService)
            .Returns(_testProcessResult);

        _scrapingResultProcessor.ProcessResult(_testProcessResult[0].Item2.Value, _testProcessResult[0].Item1)
            .Returns(_testNewsArticles);
    }
    
    [Fact]
    public async Task Collect_WhenSuccess_ReturnsListOfNewsArticles()
    {
        // Arrange
        List<(NewsArticle, ErrorOr<string>)> testProcessResult =
        [
            new ValueTuple<NewsArticle, ErrorOr<string>>()
            {
                Item1 = NewsArticleFixtureBase.Articles[0][0],
                Item2 = Errors.ExternalServiceProvider.Scraper.ScrapingRequestError("")
            }
        ];
        
        _scrapingProcessor.BatchProcess(_testNewsArticles, _scrapingService)
            .Returns(testProcessResult);
        
        // Act
        var result = await _sut.Collect(_scrapingService, _testNewsArticles);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value[0].Error.Should().BeEquivalentTo(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError("").Description);
    }
    
    [Fact]
    public async Task Collect_WhenScrapingProducesError_WritesErrorToArticle()
    {
        // Arrange
        

        // Act
        var result = await _sut.Collect(_scrapingService, _testNewsArticles);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_testNewsArticles);
    }
    
    [Fact]
    public async Task Collect_WhenScrapingFails_ReturnsError()
    {
        // Arrange

        _scrapingProcessor.BatchProcess(_testNewsArticles, _scrapingService)
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError("Failed to scrape"));

        // Act
        var result = await _sut.Collect(_scrapingService, _testNewsArticles);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError("Failed to scrape"));
    }
}