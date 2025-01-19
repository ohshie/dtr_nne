using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.NewsCollector;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ScrapingManager;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures.NewsArticleFixtures;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsEditor.TestNewsParser.TestNewsCollector;

public class TestNewsCollector
{
    public TestNewsCollector()
    {
        _mockLogger = new Mock<ILogger<NewsCollector>>();
        _mockScrapingManager = new Mock<IScrapingManager>();
        _mockNewsArticleRepository = new Mock<INewsArticleRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork<INneDbContext>>();
        _mockScraper = new Mock<IScrapingService>();
        _mockTranslator = new Mock<ITranslatorService>();
        
        _mockOutlets = NewsOutletFixtureBase.Outlets[0];
        _mockArticles = NewsArticleFixtureBase.Articles[1];
        _mockTranslatedHeadlines = NewsArticleFixtureBase.Articles[1]
            .Select(na => na.ArticleContent!.Headline).ToList();

        var faker = new Bogus.Faker();

        BasicSetup();

        _sut = new Mock<NewsCollector>(
            _mockLogger.Object,
            _mockScrapingManager.Object,
            _mockNewsArticleRepository.Object,
            _mockUnitOfWork.Object) 
            { CallBase = true };
    }

    private readonly Mock<NewsCollector> _sut;
    private readonly Mock<ILogger<NewsCollector>> _mockLogger;
    private readonly Mock<IScrapingManager> _mockScrapingManager;
    private readonly Mock<INewsArticleRepository> _mockNewsArticleRepository;
    private readonly Mock<IUnitOfWork<INneDbContext>> _mockUnitOfWork;
    private readonly Mock<IScrapingService> _mockScraper;
    private readonly Mock<ITranslatorService> _mockTranslator;
    private readonly List<NewsOutlet> _mockOutlets;
    private readonly List<NewsArticle> _mockArticles;
    private readonly List<Headline> _mockTranslatedHeadlines;

    private void BasicSetup()
    {
        _mockScrapingManager.Setup(manager => 
            manager.BatchProcess(_mockOutlets, _mockScraper.Object))
            .ReturnsAsync(_mockArticles);

        _mockNewsArticleRepository.Setup(repo => 
            repo.GetLatestResults())
            .ReturnsAsync(new List<NewsArticle>());

        _mockTranslator.Setup(translator =>
            translator.Translate(It.IsAny<List<Headline>>()))
            .ReturnsAsync(_mockTranslatedHeadlines);

        _mockUnitOfWork.Setup(uow => 
            uow.Save().Result)
            .Returns(true);
    }
    
    [Fact]
    public async Task Collect_WhenSuccess_ReturnsProcessedArticles()
    {
        // Assemble
        
        // Act
        var result = await _sut.Object.Collect(_mockScraper.Object, 
            _mockTranslator.Object, _mockOutlets);

        // Assert 
        _mockScrapingManager.Verify(manager => manager.BatchProcess(_mockOutlets, _mockScraper.Object), Times.Once);
        _mockNewsArticleRepository.Verify(repository => repository.AddRange(_mockArticles), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_mockArticles);
    }

    [Fact]
    public async Task Collect_WhenBatchProcessError_ReturnsError()
    {
        // Assemble
        _mockScrapingManager
            .Setup(manager => manager.BatchProcess(_mockOutlets, _mockScraper.Object).Result)
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));

        // Act
        var result = await _sut.Object.Collect(_mockScraper.Object, 
            _mockTranslator.Object, _mockOutlets);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
    }
    
    [Fact]
    public async Task Collect_WhenTranslationFails_ReturnsArticlesWithError()
    {
        // Arrange
        _mockTranslator
            .Setup(translator => translator.Translate(It.IsAny<List<Headline>>()).Result)
            .Returns(Errors.Translator.Service.NoHeadlineProvided);

        // Act
        var result = await _sut.Object
            .Collect(_mockScraper.Object, _mockTranslator.Object, _mockOutlets);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().AllSatisfy(article =>
            article.Error.Should().Be(Errors.Translator.Service.NoHeadlineProvided.Description));
    }
    
    [Fact]
    public async Task TranslateHeadlines_WhenSuccessful_UpdatesArticleHeadlines()
    {
        // Act
        var result = await _sut.Object.TranslateHeadlines(_mockTranslator.Object, _mockArticles);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().AllSatisfy(article =>
            article.ArticleContent!.Headline.TranslatedHeadline.Should().NotBeNullOrEmpty());
    }
    
    [Fact]
    public async Task FilterDuplicates_WhenFoundDuplicate_Filters()
    {
        // Arrange
        _mockNewsArticleRepository
            .Setup(repo => repo.GetLatestResults().Result)
            .Returns(new List<NewsArticle>([_mockArticles[0]]));

        // Act
        var result = await _sut.Object
            .FilterDuplicates(_mockArticles);

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(a => a.Uri == _mockArticles.First().Uri);
    }
    
    [Fact]
    public async Task FilterDuplicates_WhenNotFoundDuplicate_ReturnsSameList()
    {
        // Arrange
        _mockNewsArticleRepository
            .Setup(repo => repo.GetLatestResults().Result)
            .Returns([]);

        // Act
        var result = await _sut.Object
            .FilterDuplicates(_mockArticles);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(_mockArticles);
    }
}