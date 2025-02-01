using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.ScrapingProcessor;

public interface IScrapingProcessor
{
    public Task<ErrorOr<List<(T, ErrorOr<string>)>>> BatchProcess<T>(List<T> entities, IScrapingService service)
        where T : IScrapableEntity;
}