using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Application.Services.NewsEditor.NewsParser;
using dtr_nne.Application.Services.NewsEditor.NewsParser.NewsCollector;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures.NewsArticleFixtures;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsEditor.TestNewsParser;

public class TestNewsParser
{
    public TestNewsParser()
    {
        var faker = new Bogus.Faker();
        
        _mockServiceProvider = new();
        _mockArticleMapper = new();
        _mockNewsOutletRepository = new();
        _mockContentCollector = new();
        _mockNewsCollector = new();
        _mockScrapingService = new();
        _mockTranslatorService = new();
        _mockNewsOutlets = NewsOutletFixtureBase.Outlets[1];
        _mockNewsArticles = NewsArticleFixtureBase.Articles[1];

        _mockBaseNewsArticleDto = new()
        {
            Uri = new Uri(_mockNewsOutlets.First(no => no.InUse).Website+"/"+faker.Lorem.Word())
        };

        _mockNewsArticles[0].Uri = new Uri(_mockNewsOutlets.First(no => no.InUse).Website + "/" + faker.Lorem.Word());

        _mockNewsArticleDto = new()
        {
            Uri = _mockNewsArticles[0].Uri,
            Body = _mockNewsArticles[0].ArticleContent!.Body,
            OriginalHeadline = _mockNewsArticles[0].ArticleContent!.Headline.OriginalHeadline,
            TranslatedHeadline = _mockNewsArticles[0].ArticleContent!.Headline.TranslatedHeadline
        };

        BaseSetup();
        
        _sut = new Mock<NewsParser>(new Mock<ILogger<NewsParser>>().Object, 
                _mockServiceProvider.Object,
                _mockArticleMapper.Object,
                _mockNewsOutletRepository.Object,
                _mockContentCollector.Object,
                _mockNewsCollector.Object) 
            { CallBase = true };
    }

    private readonly Mock<NewsParser> _sut;
    private readonly Mock<IExternalServiceProvider> _mockServiceProvider;
    private readonly Mock<IArticleMapper> _mockArticleMapper;
    private readonly Mock<INewsOutletRepository> _mockNewsOutletRepository;
    private readonly Mock<IContentCollector> _mockContentCollector;
    private readonly Mock<INewsCollector> _mockNewsCollector;
    private readonly Mock<IScrapingService> _mockScrapingService;
    private readonly Mock<ITranslatorService> _mockTranslatorService;

    private readonly List<NewsOutlet> _mockNewsOutlets;
    private readonly List<NewsArticle> _mockNewsArticles;
    private readonly NewsArticleDto _mockNewsArticleDto;
    private readonly BaseNewsArticleDto _mockBaseNewsArticleDto;

    private void BaseSetup()
    {
        _mockServiceProvider
            .Setup(provider => provider.Provide(ExternalServiceType.Scraper, ""))
            .Returns(_mockScrapingService.Object);
        
        _mockServiceProvider
            .Setup(provider => provider.Provide(ExternalServiceType.Translator, ""))
            .Returns(_mockTranslatorService.Object);

        _mockNewsOutletRepository
            .Setup(repository => repository.GetAll().Result)
            .Returns(_mockNewsOutlets);

        _mockNewsCollector
            .Setup(collector =>
                collector.Collect(_mockScrapingService.Object, _mockTranslatorService.Object, It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(_mockNewsArticles);

        _mockContentCollector
            .Setup(collector => collector.Collect(_mockScrapingService.Object, _mockNewsArticles).Result)
            .Returns(_mockNewsArticles);

        _mockArticleMapper
            .Setup(mapper => mapper.MassNewsArticleToDto(_mockNewsArticles))
            .Returns([new NewsArticleDto(),new NewsArticleDto(), new NewsArticleDto()]);

        _mockArticleMapper
            .Setup(mapper => mapper.BaseNewsArticleDtoToNewsArticle(_mockBaseNewsArticleDto))
            .Returns(_mockNewsArticles[0]);

        _mockArticleMapper
            .Setup(mapper => mapper.NewsArticleToDto(_mockNewsArticles[0]))
            .Returns(_mockNewsArticleDto);
    }

    [Fact]
    public async Task ExecuteBatchParse_OnSuccess_Returns()
    {
        // Assemble

        // Act
        var result = await _sut.Object.ExecuteBatchParse();

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Count.Should().Be(3);
        _mockNewsCollector.Verify(x => 
                x.Collect(_mockScrapingService.Object, _mockTranslatorService.Object, It.IsAny<List<NewsOutlet>>()), 
            Times.Once);
    }
    
    [Fact]
    public async Task ExecuteBatchParse_NotFullProcess_OnSuccess_Returns()
    {
        // Assemble

        // Act
        var result = await _sut.Object.ExecuteBatchParse(fullProcess: false);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Count.Should().Be(3);
        _mockNewsCollector.Verify(x => 
                x.Collect(_mockScrapingService.Object, _mockTranslatorService.Object, It.IsAny<List<NewsOutlet>>()), 
            Times.Once);
    }
    
    [Fact]
    public async Task ExecuteBatchParse_WhenNoScrapingService_ReturnsError()
    {
        // Arrange
        _mockServiceProvider
            .Setup(provider => provider.Provide(ExternalServiceType.Scraper, ""))
            .Returns(((IExternalService?)null)!);

        // Act
        var result = await _sut.Object.ExecuteBatchParse();

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);
    }
    
    [Fact]
    public async Task ExecuteBatchParse_WhenNoTranslatorService_ReturnsError()
    {
        // Arrange
        _mockServiceProvider
            .Setup(provider => provider.Provide(ExternalServiceType.Translator, ""))
            .Returns((IExternalService?)null!);

        // Act
        var result = await _sut.Object.ExecuteBatchParse();

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);
    }
    
    [Fact]
    public async Task ExecuteBatchParse_WhenNoActiveOutlets_ReturnsError()
    {
        // Arrange
        _mockNewsOutletRepository
            .Setup(repository => repository.GetAll().Result)
            .Returns([]);

        // Act
        var result = await _sut.Object.ExecuteBatchParse();

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));
    }
    
    [Fact]
    public async Task ExecuteBatchParse_WhenNewsCollectorFails_ReturnsError()
    {
        // Arrange
        _mockNewsCollector
            .Setup(collector =>
                collector.Collect(_mockScrapingService.Object, _mockTranslatorService.Object, It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));

        // Act
        var result = await _sut.Object.ExecuteBatchParse();

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
    }

    
    [Fact]
    public async Task Execute_OnSuccess_Returns()
    {
        // Assemble
        _mockContentCollector
            .Setup(collector => collector.Collect(_mockScrapingService.Object,
                new List<NewsArticle>() { _mockNewsArticles[0] }).Result)
            .Returns(_mockNewsArticles.Take(1).ToList());

        // Act
        var result = await _sut.Object.Execute(_mockBaseNewsArticleDto);

        // Assert 
        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_WhenNoScrapingSerivce_ReturnsError()
    {
        // Assemble
        _mockServiceProvider
            .Setup(provider => provider.Provide(ExternalServiceType.Scraper, ""))
            .Returns((IScrapingService?)null!);
        // Act
        var result = await _sut.Object.Execute(_mockBaseNewsArticleDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);
    }
    
    [Fact]
    public async Task Execute_WhenNoMatchingOutlet_ReturnsError()
    {
        // Arrange
        var invalidUri = new Uri("https://invalid-outlet.com/article");
        var invalidDto = new BaseNewsArticleDto { Uri = invalidUri };
        _mockArticleMapper
            .Setup(mapper => mapper.BaseNewsArticleDtoToNewsArticle(invalidDto))
            .Returns(new NewsArticle { Uri = invalidUri });

        // Act
        var result = await _sut.Object.Execute(invalidDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ManagedEntities.NewsOutlets.MatchFailed);
    }
    
    [Fact]
    public async Task Execute_WhenContentCollectorFails_ReturnsError()
    {
        // Arrange
        _mockContentCollector
            .Setup(collector => collector.Collect(_mockScrapingService.Object,
                It.IsAny<List<NewsArticle>>()).Result)
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));

        // Act
        var result = await _sut.Object.Execute(_mockBaseNewsArticleDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
    }
    
    [Fact]
    public async Task FullProcess_WhenNoArticlesCollected_ReturnsEmptyList()
    {
        // Arrange
        _mockNewsCollector
            .Setup(collector =>
                collector.Collect(_mockScrapingService.Object, _mockTranslatorService.Object, It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(new List<NewsArticle>());

        // Act
        var result = await _sut.Object.FullProcess(_mockScrapingService.Object, _mockTranslatorService.Object, _mockNewsOutlets);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
    
    [Fact]
    public async Task RequestOutlets_WhenRepositoryReturnsNull_ReturnsError()
    {
        // Arrange
        _mockNewsOutletRepository
            .Setup(repository => repository.GetAll().Result)
            .Returns((List<NewsOutlet>?)null);

        // Act
        var result = await _sut.Object.RequestOutlets();

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));
    }

    [Fact]
    public void RequestScraper_WhenProviderThrows_ShouldReturnNull()
    {
        // Assemble
        _mockServiceProvider
            .Setup(provider => provider.Provide(ExternalServiceType.Scraper, ""))
            .Throws(new Exception());

        // Act
        var result = _sut.Object.RequestScraper();

        // Assert 
        result.Should().Be(null);
    }
    
    [Fact]
    public void RequestTranslator_WhenProviderThrows_ShouldReturnNull()
    {
        // Assemble
        _mockServiceProvider
            .Setup(provider => provider.Provide(ExternalServiceType.Translator, ""))
            .Throws(new Exception());

        // Act
        var result = _sut.Object.RequestTranslator();

        // Assert 
        result.Should().Be(null);
    }
}