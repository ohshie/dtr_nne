using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.NewsSearcher;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsEditor;

public class TestNewsSearcher
{
    public TestNewsSearcher()
    {
        Mock<ILogger<NewsSearcher>> mockLogger = new();
        _mockScrapingManager = new Mock<IScrapingManager>();
        _mockNewsOutletRepository = new Mock<INewsOutletRepository>();
        _mockNewsArticleRepository = new Mock<INewsArticleRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork<INneDbContext>>();
        _mockScrapingService = new Mock<IScrapingService>();
        
        var faker = new Bogus.Faker();

        _mockOutlets = NewsOutletFixtureBase.Outlets[1];
        
        _mockArticles = new List<NewsArticle>
        {
            new() { Id = faker.Random.Number(), Uri = new Uri(faker.Internet.Url()) },
            new() { Id = faker.Random.Number(), Uri = new Uri(faker.Internet.Url()) }
        };
        
        BasicSetup();

        _sut = new NewsSearcher(
            mockLogger.Object,
            _mockScrapingManager.Object,
            _mockNewsOutletRepository.Object,
            _mockNewsArticleRepository.Object,
            _mockUnitOfWork.Object
        );
    }
    
    private readonly NewsSearcher _sut;
    private readonly Mock<IScrapingManager> _mockScrapingManager;
    private readonly Mock<INewsOutletRepository> _mockNewsOutletRepository;
    private readonly Mock<INewsArticleRepository> _mockNewsArticleRepository;
    private readonly Mock<IUnitOfWork<INneDbContext>> _mockUnitOfWork;
    private readonly Mock<IScrapingService> _mockScrapingService;
    private readonly List<NewsOutlet> _mockOutlets;
    private readonly List<NewsArticle> _mockArticles;
    
    private void BasicSetup()
    {
        _mockNewsOutletRepository
            .Setup(repo => repo.GetAll())
            .ReturnsAsync(_mockOutlets);

        _mockScrapingManager
            .Setup(manager => manager.BatchProcess(_mockOutlets, _mockScrapingService.Object))
            .ReturnsAsync(_mockArticles);

        _mockNewsArticleRepository
            .Setup(repo => repo.GetSpecificAmount(It.IsAny<int>()))
            .ReturnsAsync(new List<NewsArticle>());
    }
    
    [Fact]
    public async Task CollectNews_WhenNoOutletsFound_ReturnsError()
    {
        // Arrange
        _mockNewsOutletRepository
            .Setup(repo => repo.GetAll())
            .ReturnsAsync((List<NewsOutlet>)null!);

        // Act
        var result = await _sut.CollectNews(_mockScrapingService.Object);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.NewsOutlets.NotFoundInDb);
    }
    
    [Fact]
    public async Task CollectNews_WhenScrapingFails_ReturnsError()
    {
        // Arrange
        _mockScrapingManager
            .Setup(manager => manager.BatchProcess(_mockOutlets, _mockScrapingService.Object))
            .ReturnsAsync(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));

        // Act
        var result = await _sut.CollectNews(_mockScrapingService.Object);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
    }

    [Fact]
    public async Task CollectNews_WhenNoNewArticlesAfterFiltering_ReturnsError()
    {
        // Arrange
        var existingArticles = _mockArticles.ToList();
        _mockNewsArticleRepository
            .Setup(repo => repo.GetSpecificAmount(It.IsAny<int>()))
            .ReturnsAsync(existingArticles);

        // Act
        var result = await _sut.CollectNews(_mockScrapingService.Object);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.NewsAticles.NoNewNewsArticles);
    }
    
    [Fact]
    public async Task CollectNews_WhenSuccessful_SavesAndReturnsNewArticles()
    {
        // Arrange
        var newArticles = _mockArticles.ToList();

        // Act
        var result = await _sut.CollectNews(_mockScrapingService.Object);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(newArticles);
        _mockNewsArticleRepository.Verify(repo => repo.AddRange(newArticles), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
    }
    
    [Fact]
    public async Task FilterDuplicates_WhenNoExistingArticles_ReturnsAllIncomingArticles()
    {
        // Act
        var result = await _sut.FilterDuplicates(_mockArticles);

        // Assert
        result.Should().BeEquivalentTo(_mockArticles);
    }
    [Fact]
    public async Task FilterDuplicates_WhenDuplicatesExist_ReturnsOnlyNewArticles()
    {
        // Arrange
        var existingArticle = _mockArticles[0];
        var newArticle = _mockArticles[1];
        _mockNewsArticleRepository
            .Setup(repo => repo.GetSpecificAmount(It.IsAny<int>()))
            .ReturnsAsync(new List<NewsArticle> { existingArticle });

        // Act
        var result = await _sut.FilterDuplicates(_mockArticles);

        // Assert
        result.Should().ContainSingle();
        result.Should().ContainEquivalentOf(newArticle);
        result.Should().NotContainEquivalentOf(existingArticle);
    }
}