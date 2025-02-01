using dtr_nne.Application.DTO.Article;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ScrapableEntities;

namespace dtr_nne.Application.Mapper;

public interface IArticleMapper
{
    public NewsArticleDto NewsArticleToDto(NewsArticle article);

    public List<NewsArticleDto> MassNewsArticleToDto(List<NewsArticle> articles);
    public NewsArticle BaseNewsArticleDtoToNewsArticle(BaseNewsArticleDto articleDto);
    public ArticleContent DtoToArticleContent(ArticleContentDto articleContentDto);
    public ArticleContentDto ArticleContentToDto(ArticleContent article);
}