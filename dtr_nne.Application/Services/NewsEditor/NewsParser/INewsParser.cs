using dtr_nne.Application.DTO.Article;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser;

public interface INewsParser
{
    public Task<ErrorOr<List<NewsArticleDto>>> ExecuteBatchParse(bool fullProcess = true, string cutOffTime = "");
    public Task<ErrorOr<NewsArticleDto>> Execute(BaseNewsArticleDto articleDto);
}