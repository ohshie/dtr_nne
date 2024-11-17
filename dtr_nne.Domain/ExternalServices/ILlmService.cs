using dtr_nne.Domain.Entities;
using ErrorOr;

namespace dtr_nne.Domain.ExternalServices;

public interface ILlmService : IExternalService
{
    public Task<ErrorOr<Article>> ProcessArticleAsync(Article article, string apiKey);
}