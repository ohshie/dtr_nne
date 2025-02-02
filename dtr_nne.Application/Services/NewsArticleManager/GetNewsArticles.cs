using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.Repositories;

namespace dtr_nne.Application.Services.NewsArticleManager;

public class GetNewsArticles(ILogger<GetNewsArticles> logger,
    INewsArticleRepository repository,
    IManagedEntityMapper mapper) : IGetNewsArticles
{
    public async Task<ErrorOr<List<NewsArticleDto>>> GetAll()
    {
        logger.LogInformation("Serving all currently saved openAi assistants");
        if (await repository.GetAllWithChildren() is not { } articles || articles.Count < 1)
        {
            return Errors.ManagedEntities.NotFoundInDb(typeof(NewsArticle));
        }

        var articleDtos = mapper.EntityToDto<NewsArticle, NewsArticleDto>(articles);
        return articleDtos;
    }
}

public interface IGetNewsArticles
{
    Task<ErrorOr<List<NewsArticleDto>>> GetAll();
}