using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;

namespace Tests.Systems.Mappers;

public class TestArticleMapper
{
    public TestArticleMapper()
    {
        var faker = new Bogus.Faker();
        _testArticleDto = new()
        {
            Body = faker.Lorem.Paragraph()
        };

        _testArticle = new()
        {
            Body = _testArticleDto.Body
        };
        
        _sut = new();
    }
    
    private readonly ArticleMapper _sut;
    private readonly ArticleDto _testArticleDto;
    private readonly Article _testArticle;
    
    [Fact]
    public void Map_DtoToArticle_EnsuresSameIdAndName()
    {
        // assemble

        // act
        var article = _sut.DtoToArticle(_testArticleDto);
        
        // assert
        article.Should().BeOfType<Article>();
        _testArticleDto.Body.Should().Match(body => body == _testArticleDto.Body);
    }
    
    [Fact]
    public void Map_ArticleToDto_EnsuresSameIdAndName()
    {
        // assemble

        // act
        var articleDto = _sut.ArticleToDto(_testArticle);
        
        // assert
        articleDto.Should().BeOfType<ArticleDto>();
        _testArticleDto.Body.Should().Match(body => body == _testArticle.Body);
    }
}