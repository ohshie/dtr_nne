using dtr_nne.Application.DTO.Article;
using dtr_nne.Domain.Entities;
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
            Uri = article.Uri,

            Themes = article.NewsOutlet!.Themes,

            OriginalHeadline = article.ArticleContent!.Headline.OriginalHeadline,
            TranslatedHeadline = article.ArticleContent.Headline.TranslatedHeadline,

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
                    OriginalHeadline = articleDto.OriginalHeadline,
                    TranslatedHeadline = articleDto.TranslatedHeadline
                }
            },
            
            Uri = articleDto.Uri,
        };
    }
    
    public partial ArticleContent DtoToArticleContent(ArticleContentDto articleContentDto);
    public partial ArticleContentDto ArticleContentToDto(ArticleContent articleContent);
}