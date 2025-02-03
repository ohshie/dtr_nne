using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.Services.NewsArticleManager;

public class DeleteNewsArticle(ILogger<DeleteNewsArticle> logger, 
    IManagedEntityMapper managedEntityMapper,
    INewsArticleRepository repository, 
    IUnitOfWork<INneDbContext> unitOfWork) : IDeleteNewsArticle
{
    public async Task<ErrorOr<List<NewsArticleDto>>> Delete(List<BaseNewsArticleDto> articles)
    {
        if (await repository.GetAllWithChildren() is not { } existingArticles)
        {
            logger.LogWarning("Currently there are not saved News Articles");
            return Errors.ManagedEntities.NotFoundInDb(articles.GetType());
        }
        
        var mappedArticles = managedEntityMapper
            .DtoToEntity<NewsArticle, BaseNewsArticleDto>(articles);

        var matchedArticles = FindRequestedArticles(mappedArticles, existingArticles);
        if (matchedArticles.Count < 1)
        {
            logger.LogWarning("No saved articles match provided news articles by Id");
            return Errors.ManagedEntities.NotFoundInDb(articles.GetType());
        }

        if (await PerformDataOperation(matchedArticles) is { } result)
            return result;

        var deletedArticles = managedEntityMapper.EntityToDto<NewsArticle, NewsArticleDto>(matchedArticles);
        return deletedArticles;
    }

    private List<NewsArticle> FindRequestedArticles(List<NewsArticle> incomingArticles,
        List<NewsArticle> savedArticles)
    {
        var matchedaArticles = savedArticles
            .Where(ie => 
                incomingArticles
                    .Select(e => e.Id)
                    .Contains(ie.Id))
            .ToList();

        return matchedaArticles;
    }

    private async Task<Error?> PerformDataOperation(List<NewsArticle> articles)
    {
        var deleteSuccess = repository.RemoveArticleRange(articles);
        if (!deleteSuccess)
        {
            logger.LogWarning("Error occured while attempting to remove news articles from Db");
            return Errors.DbErrors.RemovingFailed;
        }
        
        var changesSave = await unitOfWork.Save();
        if (changesSave) return null;
        
        logger.LogWarning("No saved articles match provided news articles by Id");
        return Errors.DbErrors.UnitOfWorkSaveFailed;

    }
}

public interface IDeleteNewsArticle
{
    Task<ErrorOr<List<NewsArticleDto>>> Delete(List<BaseNewsArticleDto> articles);
}