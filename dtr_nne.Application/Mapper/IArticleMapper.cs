using dtr_nne.Application.DTO.Article;
using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.Mapper;

public interface IArticleMapper
{
    public ArticleContent DtoToArticle(ArticleContentDto articleContentDto);
    public ArticleContentDto ArticleToDto(ArticleContent article);
}