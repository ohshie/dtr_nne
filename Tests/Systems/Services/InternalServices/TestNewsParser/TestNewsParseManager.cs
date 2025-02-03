using Bogus;
using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.NewsEditor.NewsParser;
using dtr_nne.Domain.Entities.ScrapableEntities;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tests.Fixtures.NewsArticleFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsParser;

public class TestNewsParseManager
{
    public TestNewsParseManager()
    {
        _articleMapper = Substitute.For<IArticleMapper>();
        _batchNewsParser = Substitute.For<IBatchNewsParser>();
        _newsParser = Substitute.For<INewsParser>();
        
        _sut = new(Substitute.For<ILogger<NewsParseManager>>(), 
            _batchNewsParser, _newsParser, _articleMapper);

        BasicSetup();
    }

    private readonly NewsArticleDto _tesArticleDto = new()
    {
        Uri = new Uri(Faker.Internet.UrlWithPath())
    };

    private static readonly Faker Faker = new();
    private readonly NewsParseManager _sut;
    private readonly IArticleMapper _articleMapper;
    private readonly IBatchNewsParser _batchNewsParser;
    private readonly INewsParser _newsParser;

    private void BasicSetup()
    {
        _batchNewsParser
            .ExecuteBatchParse()
            .Returns(NewsArticleFixtureBase.Articles[1]);

        _newsParser.ExecuteParse(Arg.Any<NewsArticle>())
            .Returns(NewsArticleFixtureBase.Articles[0][0]);

        _articleMapper
            .BaseNewsArticleDtoToNewsArticle(_tesArticleDto)
            .Returns(NewsArticleFixtureBase.Articles[0][0]);

        _articleMapper
            .NewsArticleToDto(NewsArticleFixtureBase.Articles[0][0])
            .Returns(_tesArticleDto);
        
        _articleMapper
            .MassNewsArticleToDto(NewsArticleFixtureBase.Articles[1])
            .Returns([]);
    }

    [Fact]
    public async Task ExecuteBatchParse_WhenParseSuccessfull_ReturnsNewsArticleDtoList()
    {
        // Assemble

        // Act
        var result = await _sut.ExecuteBatchParse();

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<List<NewsArticleDto>>();
    }
    
    [Fact]
    public async Task ExecuteBatchParse_WhenParseReturnsError_ReturnsError()
    {
        // Assemble
        _batchNewsParser
            .ExecuteBatchParse()
            .Returns(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);

        // Act
        var result = await _sut.ExecuteBatchParse();

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);
    }

    [Fact]
    public async Task ExecuteParse_WhenSuccessfull_ReturnsArticleDto()
    {
        // Assemble

        // Act
        var result = await _sut.ExecuteParse(_tesArticleDto.Uri!);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(_tesArticleDto);
    }
    
    [Fact]
    public async Task ExecuteParse_WhenErrorInNewsParser_ReturnsError()
    {
        // Assemble
        _newsParser
            .ExecuteParse(Arg.Any<NewsArticle>())
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));

        // Act
        var result = await _sut.ExecuteParse(_tesArticleDto.Uri!);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
    }
}