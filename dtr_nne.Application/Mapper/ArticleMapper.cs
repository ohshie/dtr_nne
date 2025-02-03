using dtr_nne.Application.DTO.Article;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using Riok.Mapperly.Abstractions;

namespace dtr_nne.Application.Mapper;

[Mapper]
public partial class ArticleMapper : IArticleMapper
{
    public NewsArticleDto NewsArticleToDto(NewsArticle article)
    {
        return new NewsArticleDto
        {
            Id = article.Id,
            Error = article.Error,
            Uri = article.Website,

            Themes = article.NewsOutlet!.Themes,
            
            Header = article.ArticleContent!.Headline.OriginalHeadline,
            TranslatedHeader = article.ArticleContent.Headline.TranslatedHeadline,
            Body = article.ArticleContent.Body,
            Copyrights = article.ArticleContent.Copyright,
            Pictures = article.ArticleContent.Images,
            Source = article.ArticleContent.Source
        };
    }

    public List<NewsArticleDto> MassNewsArticleToDto(List<NewsArticle> articles)
    {
        var articleDtos = articles.Select(NewsArticleToDto)
            .ToList();

        return articleDtos;
    }

    public NewsArticle BaseNewsArticleDtoToNewsArticle(BaseNewsArticleDto articleDto)
    {
        return new NewsArticle()
        {
            ArticleContent = new()
            {
                Headline = new Headline()
                {
                    OriginalHeadline = articleDto.Header,
                    TranslatedHeadline = articleDto.TranslatedHeader
                }
            },
            
            Website = articleDto.Uri,
        };
    }
    
    public partial ArticleContent DtoToArticleContent(ArticleContentDto articleContentDto);
    public partial ArticleContentDto ArticleContentToDto(ArticleContent articleContent);
}