using System.Net;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Infrastructure.ExternalServices.ScrapingServices;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

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

        _testUri = new Uri("https://test.com");
        _testCssSelector = ".test-selector";

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
    private readonly Uri _testUri;
    private readonly string _testCssSelector;

    internal void DefaultSetup()
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
        var result = await _sut.ScrapeWebsiteWithRetry(_testUri, _testCssSelector);

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
        var result = await _sut.ScrapeWebsiteWithRetry(_testUri, _testCssSelector);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError("Cannot access a disposed object.\nObject name: 'System.Net.Http.HttpClient'."));
    }
}