using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using ErrorOr;
using OpenAI.Chat;

namespace dtr_nne.Infrastructure.ExternalServices.LlmServices;

internal class OpenAiService : GenericLlmService<ChatClient, List<ChatMessage>, ChatCompletion>, IOpenAiService
{
    public override Task<ErrorOr<Article>> RewriteAsync(Article article, LlmApi apiKey)
    {
        throw new NotImplementedException();
    }

    internal override async Task<ErrorOr<ChatCompletion>> ExecuteRequest(List<ChatMessage> request)
    {
        throw new NotImplementedException();
    }
}