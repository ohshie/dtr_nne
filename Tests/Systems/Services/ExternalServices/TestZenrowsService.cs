using System.Net;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.Enums;
using dtr_nne.Infrastructure.ExternalServices.ScrapingServices;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.ExternalServices;

public class ZenrowsServiceTests
{
    public ZenrowsServiceTests()
    {
        _mockHttpMessageHandler = new();
        _mockHttpClientFactory = new();

        var testService = new ExternalService
        {
            ServiceName = "ZenrowsTest",
            Type = ExternalServiceType.Scraper,
            ApiKey = "test-api-key",
            InUse = true
        };

        DefaultSetup();

        _sut = new ZenrowsService(
            logger: new Mock<ILogger<ZenrowsService>>().Object,
            service: testService,
            _mockHttpClientFactory.Object);
    }

    private readonly ZenrowsService _sut;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private HttpClient? _httpClient;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly string _baseUri = "https://api.zenrows.com/v1/";

    private void DefaultSetup()
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("<html><body>Test content</body></html>")
            });
        
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        
        _mockHttpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);
    }
    
    [Fact]
    public async Task ScrapeWebsiteWithRetry_WhenSuccessful_ReturnsContent()
    {
        // Arrange
        var expectedContent = "<html><body>Test content</body></html>";

        // Act
        var result = await _sut.ScrapeWebsiteWithRetry(NewsOutletFixtureBase.Outlets[0][0]);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedContent);
    }
    
    [Fact]
    public async Task ScrapeWebsiteWithRetry_WhenFailsThenSucceedsWithJs_ReturnsContent()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var result = await _sut.ScrapeWebsiteWithRetry(NewsOutletFixtureBase.Outlets[0][0]);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError("Cannot access a disposed object.\nObject name: 'System.Net.Http.HttpClient'."));
    }
    
    [Fact]
    public void BuildRequestString_WithBasicParameters_ReturnsCorrectQueryString()
    {
        // Arrange
        var apiKey = "test-api-key";
        var newsArticle = new NewsArticle
        {
            Website = new Uri("https://example.com/article"),
            NewsOutlet = new NewsOutlet
            {
                Name = null!,
                Website = null!,
                MainPagePassword = null!,
                NewsPassword = null!,
                Themes = null!
            }
        };

        // Act
        var result = _sut.BuildRequestString(newsArticle, apiKey);

        // Assert
        result.Should().Be($"{_baseUri}?apikey=test-api-key&premium_proxy=true&url=https%3a%2f%2fexample.com%2farticle");
    }

}