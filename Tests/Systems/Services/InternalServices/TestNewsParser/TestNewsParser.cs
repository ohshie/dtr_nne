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

public class TestNewsParser
{
    public TestNewsParser()
    {
        _newsParseHelper = Substitute.For<INewsParseHelper>();
        _articleParseProcessor = Substitute.For<IArticleParseProcessor>();

        _scrapingService = Substitute.For<IScrapingService>();
        _translatorService = Substitute.For<ITranslatorService>();
        
        _sut = new NewsParser(_newsParseHelper, _articleParseProcessor);

        SetupMocks();
    }

    private readonly NewsParser _sut;
    private readonly INewsParseHelper _newsParseHelper;
    private readonly IArticleParseProcessor _articleParseProcessor;
    private readonly IScrapingService _scrapingService;
    private readonly ITranslatorService _translatorService;
    private readonly NewsArticle _testArticle = NewsArticleFixtureBase.Articles[0][0];

    private void SetupMocks()
    {
        _newsParseHelper.RequestScraper()
            .Returns(_scrapingService);

        _newsParseHelper.RequestTranslator()
            .Returns(_translatorService);

        _newsParseHelper
            .RequestOutlets(_testArticle)
            .Returns(new List<NewsOutlet>([NewsOutletFixtureBase.Outlets[0][0]]));

        _articleParseProcessor
            .Collect(_scrapingService, Arg.Any<List<NewsArticle>>())
            .Returns(new List<NewsArticle>([_testArticle]));

        _translatorService
            .Translate(Arg.Any<List<Headline>>())
            .Returns(new List<Headline>([_testArticle.ArticleContent!.Headline]));
    }

    [Fact]
    public async Task ExecuteParse_WhenAllServicesAvailable_ReturnsProcessedArticle()
    {
        // Arrange 
        
        // Act
        var result = await _sut.ExecuteParse(_testArticle);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_testArticle);
    }

    [Fact]
    public async Task ExecuteParse_WhenNoScraperAvailable_ReturnsError()
    {
        // Arrange
        _newsParseHelper.RequestScraper()
            .Returns((IScrapingService)null!);

        // Act
        var result = await _sut.ExecuteParse(_testArticle);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);
    }

    [Fact]
    public async Task ExecuteParse_WhenNoTranslatorAvailable_ReturnsError()
    {
        // Arrange
        _newsParseHelper.RequestTranslator()
            .Returns((ITranslatorService)null!);

        // Act
        var result = await _sut.ExecuteParse(_testArticle);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);
    }

    [Fact]
    public async Task ExecuteParse_WhenOutletRequestFails_PropagatesError()
    {
        // Arrange
        var expectedError = Errors.ExternalServiceProvider.Service.NoActiveServiceFound;
        _newsParseHelper
            .RequestOutlets(_testArticle)
            .Returns(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);

        // Act
        var result = await _sut.ExecuteParse(_testArticle);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);
    }

    [Fact]
    public async Task ExecuteParse_WhenProcessingFails_ReturnsProcessingError()
    {
        // Arrange
        _articleParseProcessor.Collect(_scrapingService, Arg.Any<List<NewsArticle>>())
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));

        // Act
        var result = await _sut.ExecuteParse(_testArticle);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
    }

    [Fact]
    public async Task ExecuteParse_WhenTranslationFails_StoresErrorInHeadline()
    {
        // Arrange
        _translatorService
            .Translate(Arg.Any<List<Headline>>())
            .Returns(Errors.Translator.Api.BadApiKey);
        
        // Act
        var result = await _sut.ExecuteParse(_testArticle);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ArticleContent!.Headline.TranslatedHeadline
            .Should().Be(Errors.Translator.Api.BadApiKey.Description);
    }
}