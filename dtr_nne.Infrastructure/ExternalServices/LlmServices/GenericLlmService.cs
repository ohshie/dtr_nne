using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using ErrorOr;

namespace dtr_nne.Infrastructure.ExternalServices.LlmServices;

internal abstract class GenericLlmService<TChat, TRequest, TResponse> : ILlmService
    where TChat : class
    where TRequest : class
    where TResponse : class
{
    public abstract Task<ErrorOr<Article>> RewriteAsync(Article article, LlmApi apiKey);
    internal abstract Task<ErrorOr<TResponse>> ExecuteRequest(TRequest request);
}