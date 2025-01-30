using dtr_nne.Domain.Entities.ScrapableEntities;
using ErrorOr;

namespace dtr_nne.Domain.ExternalServices;

public interface IScrapingService : IExternalService
{ 
    public Task<ErrorOr<string>> ScrapeWebsiteWithRetry<T>(T entity, int maxRetries = 2)
        where T : IScrapableEntity;
}