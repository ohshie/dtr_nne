using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager.MainPageScrapingResultProcessor;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsEditor
{
    public class TestScrapingManager
    {
        private readonly Mock<ILogger<ScrapingManager>> _mockLogger;
        private readonly Mock<IMainPageScrapingResultProcessor> _mockResultProcessor;
        private readonly Mock<IScrapingService> _mockScrapingService;
        private readonly Mock<ScrapingManager> _sut;
        private static readonly Bogus.Faker Faker = new();

        public TestScrapingManager()
        {
            _mockLogger = new();
            _mockResultProcessor = new Mock<IMainPageScrapingResultProcessor>();
            _mockScrapingService = new Mock<IScrapingService>();
            
            BasicSetup();

            _sut = new Mock<ScrapingManager>(_mockLogger.Object, _mockResultProcessor.Object) {CallBase = true};
        }

        private readonly List<NewsArticle> _mockArticles =
        [
            new NewsArticle
            {
                Uri = new Uri(Faker.Internet.Url()),
                NewsOutlet = NewsOutletFixtureBase.Outlets[0][0],
            }, 
            new NewsArticle
            {
                Uri = new Uri(Faker.Internet.Url()),
                NewsOutlet = NewsOutletFixtureBase.Outlets[0][0],
            }, 
            new NewsArticle
            {
                Uri = new Uri(Faker.Internet.Url()),
                NewsOutlet = NewsOutletFixtureBase.Outlets[0][0],
            }
        ];

        private void BasicSetup()
        {
            _mockScrapingService
                .Setup(s => 
                    s.ScrapeWebsiteWithRetry(It.IsAny<Uri>(), It.IsAny<string>(), false, 2).Result)
                .Returns("scraped_content");
            
            _mockResultProcessor
                .Setup(p => p.ProcessResult(It.IsAny<string>(), It.IsAny<NewsOutlet>()))
                .Returns(_mockArticles);
        }

        [Fact]
        public async Task BatchProcess_AllEntitiesProcessedSuccessfully_ReturnsNewsArticles()
        {
            // Arrange

            // Act
            var result = await _sut.Object.BatchProcess(NewsOutletFixtureBase.Outlets[0], _mockScrapingService.Object);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().HaveCount(3);
            result.Value[0].Uri.Should().Be(_mockArticles[0].Uri);
            result.Value[1].Uri.Should().Be(_mockArticles[1].Uri);
            result.Value[2].Uri.Should().Be(_mockArticles[2].Uri);
        }
        
        [Fact]
        public async Task BatchProcess_ScrapingServiceThrowsException_ReturnsError()
        {
            // Arrange
            _mockScrapingService
                .Setup(s => s.ScrapeWebsiteWithRetry(
                    It.IsAny<Uri>(), 
                    It.IsAny<string>(), 
                    It.IsAny<bool>(), 
                    It.IsAny<int>()).Result)
                .Throws(new Exception("Scraping failed"));

            // Act
            var result = await _sut.Object.BatchProcess(NewsOutletFixtureBase.Outlets[0], _mockScrapingService.Object);

            // Assert
            result.IsError.Should().BeTrue();
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
        
        [Fact]
        public async Task LoadSemaphore_RespectsConcurrencyLimit()
        {
            // Arrange
            var semaphore = new SemaphoreSlim(2);
            var entities = NewsOutletFixtureBase.Outlets[0].Take(5).ToList();
            var completedTasks = new List<DateTime>();
            var delay = TimeSpan.FromMilliseconds(100);

            _mockScrapingService
                .Setup(s => s.ScrapeWebsiteWithRetry(It.IsAny<Uri>(), It.IsAny<string>(), false, 2))
                .Returns(async () =>
                {
                    await Task.Delay(delay);
                    completedTasks.Add(DateTime.UtcNow);
                    return "scraped_content";
                });

            // Act
            var tasks = await _sut.Object.LoadSemaphore(entities, _mockScrapingService.Object, semaphore);
            await Task.WhenAll(tasks);

            // Assert
            var timeRanges = completedTasks
                .OrderBy(t => t)
                .Select((time, index) => new { Time = time, Index = index })
                .ToList();
            
            for (int i = 2; i < timeRanges.Count; i++)
            {
                var timeDiff = timeRanges[i].Time - timeRanges[i - 2].Time;
                timeDiff.Should().BeGreaterThanOrEqualTo(delay);
            }
        }
        
        [Fact]
        public async Task ProcessEntity_WithInvalidType_ThrowsNotImplementedException()
        {
            // Arrange
            var invalidEntity = new { Name = "Invalid" };

            // Act
            var act = () => _sut.Object.ProcessEntity(invalidEntity, _mockScrapingService.Object);
            
            // assert
            await act.Should().ThrowAsync<NotImplementedException>();
        }
        
        [Fact]
        public async Task ScrapeMainPage_ScrapingError_ReturnsArticleWithError()
        {
            // Arrange
            var errorMessage = "Failed to scrape";
            _mockScrapingService
                .Setup(s => s.ScrapeWebsiteWithRetry(It.IsAny<Uri>(), It.IsAny<string>(), false, 2).Result)
                .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(errorMessage));

            // Act
            var result = await _sut.Object.ScrapeMainPage(_mockScrapingService.Object, NewsOutletFixtureBase.Outlets[0][0]);

            // Assert
            result.Should().HaveCount(1);
            result[0].Error.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(errorMessage).Description);
            result[0].NewsOutlet.Should().Be(NewsOutletFixtureBase.Outlets[0][0]);
        }

    }
}