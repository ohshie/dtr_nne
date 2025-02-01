using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingProcessor;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.ExternalServices;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsParser.TestContentProcessing;

public class TestScrapingProcessor
{
    public TestScrapingProcessor()
    {
        _scrapingService = Substitute.For<IScrapingService>();

        _sut = new ScrapingProcessor(Substitute.For<ILogger<ScrapingProcessor>>());
    }

    private readonly ScrapingProcessor _sut;
    private readonly IScrapingService _scrapingService;

    private readonly List<NewsOutlet> _testNewsOutlets = NewsOutletFixtureBase.Outlets[1];
    
    [Fact]
    public async Task BatchProcess_WhenSuccess_ReturnsListOfResults()
    {
        // Arrange
        _scrapingService.ScrapeWebsiteWithRetry(Arg.Any<NewsOutlet>())
            .Returns("scraped content");

        // Act
        var result = await _sut.BatchProcess(_testNewsOutlets, _scrapingService);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(_testNewsOutlets.Count);
        result.Value[0].Item2.Value.Should().Be("scraped content");
        result.Value[1].Item2.Value.Should().Be("scraped content");
    }
    
    [Fact]
    public async Task BatchProcess_WhenScrapingFails_IncludesErrorsInResults()
    {
        // Arrange
        _scrapingService.ScrapeWebsiteWithRetry(Arg.Any<NewsOutlet>())
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError("Failed to scrape"));

        // Act
        var result = await _sut.BatchProcess(_testNewsOutlets, _scrapingService);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(3);
        result.Value[0].Item2.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError("Failed to scrape"));
        result.Value[1].Item2.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError("Failed to scrape"));
    }
    
    [Fact]
    public async Task BatchProcess_WhenConcurrencyLimitIsReached_WaitsForSemaphore()
    {
        // Arrange
        var entities = new List<NewsOutlet>
        {
            new()
            {
                Website = new Uri("http://example.com/outlet1"),
                Name = null,
                MainPagePassword = null,
                NewsPassword = null,
                Themes = null
            },
            new()
            {
                Website = new Uri("http://example.com/outlet2"),
                Name = null,
                MainPagePassword = null,
                NewsPassword = null,
                Themes = null
            },
            new()
            {
                Website = new Uri("http://example.com/outlet3"),
                Name = null,
                MainPagePassword = null,
                NewsPassword = null,
                Themes = null
            },
            new()
            {
                Website = new Uri("http://example.com/outlet4"),
                Name = null,
                MainPagePassword = null,
                NewsPassword = null,
                Themes = null
            },
            new()
            {
                Website = new Uri("http://example.com/outlet5"),
                Name = null,
                MainPagePassword = null,
                NewsPassword = null,
                Themes = null
            },
            new()
            {
                Website = new Uri("http://example.com/outlet6"),
                Name = null,
                MainPagePassword = null,
                NewsPassword = null,
                Themes = null
            }
        };

        _scrapingService.ScrapeWebsiteWithRetry(Arg.Any<NewsOutlet>())
            .Returns("Scraped Content");

        // Act
        var result = await _sut.BatchProcess(entities, _scrapingService);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(6);
        await _scrapingService.Received(6).ScrapeWebsiteWithRetry(Arg.Any<NewsOutlet>());
    }
    
    [Fact]
    public async Task BatchProcess_WhenExceptionOccurs_ReturnsError()
    {
        // Arrange
        _scrapingService.ScrapeWebsiteWithRetry(Arg.Any<NewsOutlet>())
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _sut.BatchProcess(_testNewsOutlets, _scrapingService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(new Exception("Unexpected error").Message));
    }
}