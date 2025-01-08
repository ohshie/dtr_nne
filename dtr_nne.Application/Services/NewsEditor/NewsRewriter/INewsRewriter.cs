using dtr_nne.Application.DTO.Article;

namespace dtr_nne.Application.Services.NewsEditor.NewsRewriter;

public interface INewsRewriter
{
    Task<ErrorOr<ArticleDto>> Rewrite(ArticleDto articleDto);
}