using dtr_nne.Application.DTO.Article;
using dtr_nne.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace dtr_nne.Application.Mapper;

[Mapper]
public partial class ArticleMapper : IArticleMapper
{
    public partial ArticleContent DtoToArticle(ArticleContentDto articleContentDto);
    public partial ArticleContentDto ArticleToDto(ArticleContent article);
}