using dtr_nne.Application.DTO.Article;
using dtr_nne.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace dtr_nne.Application.Mapper;

[Mapper]
public partial class ArticleMapper : IArticleMapper
{
    public partial Article DtoToArticle(ArticleDto articleDto);
    public partial ArticleDto ArticleToDto(Article article);
}