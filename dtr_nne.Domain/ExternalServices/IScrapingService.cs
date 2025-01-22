using ErrorOr;

namespace dtr_nne.Domain.ExternalServices;

public interface IScrapingService : IExternalService
{ 
    public Task<ErrorOr<string>> ScrapeWebsiteWithRetry<T>(T scrapingSettings, int maxRetries = 2);
}