using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Application.Services.NewsEditor.NewsParser;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using Tests.Fixtures.NewsArticleFixtures;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsParser;

public class TestNewsParseHelper
{
    public TestNewsParseHelper()
    {
        _provider = Substitute.For<IExternalServiceProvider>();
        _repository = Substitute.For<IRepository<NewsOutlet>>();
        _scrapingService = Substitute.For<IScrapingService>();
        _translatorService = Substitute.For<ITranslatorService>();

        _sut = new(Substitute.For<ILogger<NewsParseHelper>>(),
            _provider, _repository);

        BasicSetup();
    }

    private readonly NewsParseHelper _sut;
    private readonly IExternalServiceProvider _provider;
    private readonly IRepository<NewsOutlet> _repository;
    private readonly IScrapingService _scrapingService;
    private readonly ITranslatorService _translatorService;
    private List<NewsOutlet>? _testOutlets = NewsOutletFixtureBase.Outlets[0];
    private readonly NewsArticle _testArticle = NewsArticleFixtureBase.Articles[0][0];

    private void BasicSetup()
    {
        _testOutlets![0].InUse = true;
            
        _provider.Provide(ExternalServiceType.Scraper)
            .Returns(_scrapingService);
        
        _provider.Provide(ExternalServiceType.Translator)
            .Returns(_translatorService);

        _repository.GetAll()
            .Returns(_testOutlets);
    }

    [Fact]
    public async Task RequestScraper_WhenServiceExist_ReturnsScrapingService()
    {
        // Assemble

        // Act
        var result = await _sut.RequestScraper();

        // Assert 
        result.Should().NotBeNull();
        result.Should().Be(_scrapingService);
    }
    
    [Fact]
    public async Task RequestScraper_WhenProviderThrows_ReturnsNull()
    {
        // Assemble
        _provider.Provide(ExternalServiceType.Scraper)
            .ThrowsAsync(new Exception());
        
        // Act
        var result = await _sut.RequestScraper();

        // Assert 
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task RequestTranslator_WhenServiceExist_ReturnsScrapingService()
    {
        // Assemble

        // Act
        var result = await _sut.RequestTranslator();

        // Assert 
        result.Should().NotBeNull();
        result.Should().Be(_translatorService);
    }
    
    [Fact]
    public async Task RequestTranslator_WhenProviderThrows_ReturnsNull()
    {
        // Assemble
        _provider.Provide(ExternalServiceType.Translator)
            .ThrowsAsync(new Exception());
        
        // Act
        var result = await _sut.RequestTranslator();

        // Assert 
        result.Should().BeNull();
    }

    [Fact]
    public async Task RequestOutlets_WhenOutletsExist_ReturnNewsOutlets()
    {
        // Assemble

        // Act
        var result = await _sut.RequestOutlets();

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<List<NewsOutlet>>();
    }
    
    [Fact]
    public async Task RequestOutlets_WhenOutletsAreEmpty_ReturnError()
    {
        // Assemble
        _testOutlets = null;
        _repository.GetAll().Returns(_testOutlets);

        // Act
        var result = await _sut.RequestOutlets();

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));
    }
    
    [Fact]
    public async Task RequestOutlets_WhenLessThanOneActive_ReturnError()
    {
        // Assemble
        _repository.ClearSubstitute();

        // Act
        var result = await _sut.RequestOutlets();

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));
    }
    
    [Fact]
    public async Task RequestOutlets_WhenFilterReturnsEmptyList_ReturnError()
    {
        // Assemble
        _testArticle.Website = new Uri("https://test.com");

        // Act
        var result = await _sut.RequestOutlets(_testArticle);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));
    }
    
    [Fact]
    public async Task RequestOutlets_WhenFilterReturnsProperOutlet_ReturnOutlet()
    {
        // Assemble
        _testArticle.Website = _testOutlets![0].Website;

        // Act
        var result = await _sut.RequestOutlets(_testArticle);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_testOutlets);
    }

    [Fact]
    public void FilterNewsOutlets_WhenOutletFound_ReturnsOutlet()
    {
        // Assemble
        _testArticle.Website = _testOutlets![0].Website;

        // Act
        var result = _sut.FilterNewsOutlets(_testOutlets, _testArticle);

        // Assert 
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
    }
    
    [Fact]
    public void FilterNewsOutlets_WhenOutletNotFound_ReturnsHuh()
    {
        // Assemble

        // Act
        var result = _sut.FilterNewsOutlets(_testOutlets!, _testArticle);

        // Assert 
        result.Should().NotBeNull();
        result.Count.Should().Be(0);
    }
}