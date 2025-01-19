using ErrorOr;

namespace dtr_nne.Domain.ExternalServices;

public interface IScrapingService : IExternalService
{
    public Task<ErrorOr<string>> ScrapeWebsiteWithRetry(Uri uri, string cssSelector = "", bool alwaysJs = false, int maxRetries = 2);
}