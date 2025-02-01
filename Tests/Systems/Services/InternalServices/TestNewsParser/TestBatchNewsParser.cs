using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;
using NSubstitute;
using Tests.Fixtures.NewsArticleFixtures;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsParser;

public class TestBatchNewsParser
{
    public TestBatchNewsParser()
    {
        _newsParseProcessor = Substitute.For<INewsParseProcessor>();
        _articleParseProcessor = Substitute.For<IArticleParseProcessor>();
        _newsParseHelper = Substitute.For<INewsParseHelper>();
        _scrapingService = Substitute.For<IScrapingService>();
        _translatorService = Substitute.For<ITranslatorService>();
        
        _sut = new(_newsParseProcessor, _articleParseProcessor,_newsParseHelper);
        
        BasicSetup();
    }

    private BatchNewsParser _sut;
    private INewsParseProcessor _newsParseProcessor;
    private IArticleParseProcessor _articleParseProcessor;
    private readonly INewsParseHelper _newsParseHelper;
    private readonly IScrapingService _scrapingService;
    private readonly ITranslatorService _translatorService;
    private List<NewsArticle> _testNewsArticles = NewsArticleFixtureBase.Articles[0];
    private List<NewsArticle> _testProcessedNewsArticles = NewsArticleFixtureBase.Articles[0];
    private List<NewsOutlet> _testNewsOutlets = NewsOutletFixtureBase.Outlets[0];

    private void BasicSetup()
    {
        _newsParseHelper
            .RequestOutlets()
            .Returns(_testNewsOutlets);

        _newsParseHelper
            .RequestScraper()
            .Returns(_scrapingService);

        _newsParseHelper
            .RequestTranslator()
            .Returns(_translatorService);
        
        _newsParseProcessor
            .Collect(_scrapingService, _translatorService, _testNewsOutlets)
            .Returns(_testNewsArticles);
        
        _articleParseProcessor
            .Collect(_scrapingService, _testNewsArticles)
            .Returns(_testProcessedNewsArticles);
        
        _translatorService
            .Translate(Arg.Any<List<Headline>>())
            .Returns(_testProcessedNewsArticles.Select(a => a.ArticleContent!.Headline).ToList());
    }

    [Fact]
    public async Task ExecuteBatchParse_WhenSuccess_ReturnsListOfNewsArticles()
    {
        // Arrange

        // Act
        var result = await _sut.ExecuteBatchParse();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_testProcessedNewsArticles);
    }
    
    [Fact]
    public async Task ExecuteBatchParse_WhenNoScraperAvailable_ReturnsError()
    {
        // Arrange
        _newsParseHelper.RequestScraper()
            .Returns((IScrapingService)null!);

        // Act
        var result = await _sut.ExecuteBatchParse();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);
    }
    
    [Fact]
    public async Task ExecuteBatchParse_WhenNoTranslatorAvailable_ReturnsError()
    {
        // Arrange
        _newsParseHelper.RequestTranslator()
            .Returns((ITranslatorService)null!);

        // Act
        var result = await _sut.ExecuteBatchParse();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);
    }
    
    [Fact]
    public async Task ExecuteBatchParse_WhenOutletRequestFails_PropagatesError()
    {
        // Arrange
        var expectedError = Errors.ExternalServiceProvider.Service.NoActiveServiceFound;
        _newsParseHelper
            .RequestOutlets()
            .Returns(expectedError);

        // Act
        var result = await _sut.ExecuteBatchParse();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);
    }
    
    [Fact]
    public async Task ExecuteBatchParse_WhenProcessingFails_ReturnsProcessingError()
    {
        // Arrange
        _newsParseProcessor
            .Collect(_scrapingService, _translatorService, Arg.Any<List<NewsOutlet>>())
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));

        // Act
        var result = await _sut.ExecuteBatchParse();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
    }
    
    [Fact]
    public async Task ExecuteBatchParse_WhenTranslationFails_StoresErrorInHeadline()
    {
        // Arrange
        var expectedArticles = NewsArticleFixtureBase.Articles[0];
        var expectedError = Errors.Translator.Api.BadApiKey;

        _newsParseProcessor
            .Collect(_scrapingService, _translatorService, Arg.Any<List<NewsOutlet>>())
            .Returns(expectedArticles);

        _articleParseProcessor
            .Collect(_scrapingService, Arg.Any<List<NewsArticle>>())
            .Returns(expectedArticles);

        _translatorService
            .Translate(Arg.Any<List<Headline>>())
            .Returns(expectedError);

        // Act
        var result = await _sut.ExecuteBatchParse();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ForEach(article => 
            article.ArticleContent!.Headline.TranslatedHeadline.Should().Be(expectedError.Description));
    }
}