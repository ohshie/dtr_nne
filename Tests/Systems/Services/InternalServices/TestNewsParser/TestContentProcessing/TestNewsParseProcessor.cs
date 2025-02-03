using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingProcessor;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingResultProcessor;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tests.Fixtures.NewsArticleFixtures;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsParser.TestContentProcessing;

public class TestNewsParseProcessor
{
    public TestNewsParseProcessor()
    {
        _scrapingProcessor = Substitute.For<IScrapingProcessor>();
        _scrapingResultProcessor = Substitute.For<IScrapingResultProcessor>();
        _newsArticleRepository = Substitute.For<INewsArticleRepository>();
        _scrapingService = Substitute.For<IScrapingService>();
        _translatorService = Substitute.For<ITranslatorService>();

        BasicSetup();
        
        _sut = new NewsParseProcessor(Substitute.For<ILogger<NewsParseProcessor>>(), _scrapingProcessor, _scrapingResultProcessor, _newsArticleRepository);
    }
    
    private readonly NewsParseProcessor _sut;
    private readonly IScrapingProcessor _scrapingProcessor;
    private readonly IScrapingResultProcessor _scrapingResultProcessor;
    private readonly INewsArticleRepository _newsArticleRepository;
    private readonly IScrapingService _scrapingService;
    private readonly ITranslatorService _translatorService;
    private List<NewsOutlet> _testNewsOutlets = NewsOutletFixtureBase.Outlets[0];
    private List<NewsArticle> _testNewsArticles = NewsArticleFixtureBase.Articles[0];

    private readonly List<(NewsOutlet, ErrorOr<string>)> _testProcessResult =
    [
        new ValueTuple<NewsOutlet, ErrorOr<string>>()
        {
            Item1 = NewsOutletFixtureBase.Outlets[0][0],
            Item2 = "scraper result"
        }
    ];

    private void BasicSetup()
    {
        _scrapingProcessor.BatchProcess(_testNewsOutlets, Arg.Any<IScrapingService>())
            .Returns(_testProcessResult);

        _scrapingResultProcessor.ProcessResult(_testProcessResult[0].Item2.Value, _testProcessResult[0].Item1)
            .Returns(_testNewsArticles);

        _newsArticleRepository.GetLatestResults()
            .Returns(new List<NewsArticle>());
    }
    
    [Fact]
    public async Task Collect_WhenSuccess_ReturnsListOfNewsArticles()
    {
        // Arrange

        // Act
        var result = await _sut.Collect(_scrapingService, _translatorService, _testNewsOutlets);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_testNewsArticles);
    }
    
    [Fact]
    public async Task Collect_WhenScrapingFails_ReturnsError()
    {
        // Arrange
        _scrapingProcessor.BatchProcess(_testNewsOutlets, _scrapingService)
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));

        // Act
        var result = await _sut.Collect(_scrapingService, _translatorService, _testNewsOutlets);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
    }
    
    [Fact]
    public async Task Collect_WhenScrapingProducessErrors_WritesErrorsToArticles()
    {
        // Arrange
        var scrapeResults = new List<(NewsOutlet, ErrorOr<string>)>
        {
            (_testNewsOutlets[0], Errors.ExternalServiceProvider.Scraper.ScrapingRequestError("Failed to scrape"))
        };

        _scrapingProcessor.BatchProcess(_testNewsOutlets, _scrapingService)
            .Returns(scrapeResults);

        // Act
        var result = await _sut.Collect(_scrapingService, _translatorService, _testNewsOutlets);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value[0].Error.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError("Failed to scrape").Description);
    }
    
    [Fact]
    public async Task Collect_WhenFilteringDuplicates_ReturnsFilteredArticles()
    {
        // Arrange
        _newsArticleRepository.GetLatestResults()
            .Returns(_testNewsArticles);

        // Act
        var result = await _sut.Collect(_scrapingService, _translatorService, _testNewsOutlets);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(0); // One duplicate should be filtered out
    }
}