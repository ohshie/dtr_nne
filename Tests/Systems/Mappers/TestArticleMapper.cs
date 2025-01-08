using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;

namespace Tests.Systems.Mappers;

public class TestArticleMapper
{
    public TestArticleMapper()
    {
        var faker = new Bogus.Faker();
        _testArticleContentDto = new()
        {
            Body = faker.Lorem.Paragraph()
        };

        _testArticle = new()
        {
            Body = _testArticleContentDto.Body
        };
        
        _sut = new();
    }
    
    private readonly ArticleMapper _sut;
    private readonly ArticleContentDto _testArticleContentDto;
    private readonly ArticleContent _testArticle;
    
    [Fact]
    public void Map_DtoToArticle_EnsuresSameIdAndName()
    {
        // assemble

        // act
        var article = _sut.DtoToArticle(_testArticleContentDto);
        
        // assert
        article.Should().BeOfType<ArticleContent>();
        _testArticleContentDto.Body.Should().Match(body => body == _testArticleContentDto.Body);
    }
    
    [Fact]
    public void Map_ArticleToDto_EnsuresSameIdAndName()
    {
        // assemble

        // act
        var articleDto = _sut.ArticleToDto(_testArticle);
        
        // assert
        articleDto.Should().BeOfType<ArticleContentDto>();
        _testArticleContentDto.Body.Should().Match(body => body == _testArticle.Body);
    }
}