using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.NewsCollector;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures.NewsArticleFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsEditor.TestNewsParser.TestContentCollector;

public class TestContentCollector
{
    private readonly Mock<ContentCollector> _sut;
    private readonly Mock<IScrapingService> _mockScrapingService;
    private readonly Mock<IScrapingManager> _mockScrapingManager;
    private readonly List<NewsArticle> _mockNewsArticles;

    public TestContentCollector()
    {
        _mockScrapingService = new();
        _mockScrapingManager = new();
        _mockNewsArticles = NewsArticleFixtureBase.Articles[1];

        BasicSetup();
        
        _sut = new Mock<ContentCollector>(new Mock<ILogger<ContentCollector>>().Object, 
                _mockScrapingManager.Object) 
            { CallBase = true };
    }

    private void BasicSetup()
    {
        _mockScrapingManager.Setup(manager =>
                manager.BatchProcess(_mockNewsArticles, _mockScrapingService.Object).Result)
            .Returns(_mockNewsArticles);
    }
    
    [Fact]
    public async Task Collect_OnSuccess_ReturnsListOfArticles()
    {
        // Assemble
        
        // Act
        var result = await _sut.Object.Collect(_mockScrapingService.Object, _mockNewsArticles);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_mockNewsArticles);
    }
    
    [Fact]
    public async Task Collect_OnError_ReturnsError()
    {
        // Assemble
        _mockScrapingManager.Setup(manager =>
                manager.BatchProcess(_mockNewsArticles, _mockScrapingService.Object).Result)
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
        
        // Act
        var result = await _sut.Object.Collect(_mockScrapingService.Object, _mockNewsArticles);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
    }
}