using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using ErrorOr;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Assistants;

namespace dtr_nne.Infrastructure.ExternalServices.LlmServices;

[Experimental("OPENAI001")]
internal class OpenAiService(ILogger<OpenAiService> logger) : IOpenAiService
{
    private readonly List<List<MessageContent>> _messages = [["translate"],["process"]];
        
    private readonly Dictionary<string, string> _assistants = new()
    {
        { "rewrite", "asst_FwmZADIgvx1PNbpbE4jFZdIT" },
        { "translate", "asst_uZX8vrFAa7Cs6rxpewtXjcZS" },
        { "header", "asst_mNcZNCvZBrTGHC0CFgCQSUFt" },
        { "subheader", "asst_4tGc3wgCipY1blaiqQpA8oSR" }
    };

    readonly List<string> _processingSteps = ["rewrite", "translate", "header", "subheader"];
    
    public async Task<ErrorOr<Article>> ProcessArticleAsync(Article article, string apiKey)
    {
        var editedArticle = new EditedArticle();

        var client = new OpenAIClient(apiKey);
        var assistantClient = client.GetAssistantClient();
        var thread = await CreateThread(assistantClient, article.Body);

        foreach (var step in _processingSteps)
        {
            var assistant = await GetAssistantByPurpose(assistantClient, step);
            string runId;
            if (_processingSteps[0] == step)
            {
                runId = await RunAssistantOnThreadAsync(thread.Value.Id, 
                    assistantClient, 
                    assistant.Value, 
                    initialMessage: true);
            }
            else
            {
                runId = await RunAssistantOnThreadAsync(thread.Value.Id, assistantClient, assistant.Value);
            }
            
            WriteRunIdsToEditedArticle(editedArticle, step, runId);
        }
        
        var messages = assistantClient.GetMessages(thread.Value.Id,
            new MessageCollectionOptions { Order = MessageCollectionOrder.Descending });
        
        UpdateEditedArticle(editedArticle, messages);
        
        article.EditedArticle = editedArticle;

        return article;
    }

    private async Task<ErrorOr<AssistantThread>> CreateThread(AssistantClient assistantClient, string articleBody)
    {
        var threadOpt = new ThreadCreationOptions
        {
            InitialMessages =
            {
                articleBody   
            }
        };
        
        var thread = (await assistantClient.CreateThreadAsync(threadOpt)).Value;

        return thread;
    }
    
    private async Task<ErrorOr<Assistant>> GetAssistantByPurpose(AssistantClient client, string purpose)
    {
        if (!_assistants.TryGetValue(purpose, out var assistantId))
        {
            // todo need to change this
            return Errors.ExternalServiceProvider.Service.InvalidRequestedServiceType;
        }
        
        var clientResult = await client.GetAssistantAsync(assistantId);
        return clientResult.Value;
    }

    private async Task<string> RunAssistantOnThreadAsync(string threadId, AssistantClient assistantClient, Assistant assistant, bool initialMessage = false)
    {
        if (!initialMessage)
        {
            await assistantClient.CreateMessageAsync(threadId, MessageRole.User, _messages[0]);
        }
      
        var threadRun = await assistantClient.CreateRunAsync(threadId, assistant.Id);
        
        return await PollForRunCompletionAsync(threadId, threadRun.Value.Id, assistantClient);
    }

    private async Task<string> PollForRunCompletionAsync(string threadId, string runId, AssistantClient assistantClient, int maxAttempts = 100, int delayMilliSeconds = 500)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            await Task.Delay(delayMilliSeconds);
            var threadRun = assistantClient.GetRun(threadId, runId);

            if (threadRun.Value.Status.IsTerminal)
            {
                return runId;
            }
        }
        
        throw new TimeoutException("Assistant run did not complete in time.");
    }
    
    private void WriteRunIdsToEditedArticle(EditedArticle editedArticle, string step, string runId)
    {
        switch (step)
        {
            case "rewrite":
                editedArticle.EditedBodyRunId = runId;
                break;
            case "translate":
                editedArticle.TranslatedBodyRunId = runId;
                break;
            case "header":
                editedArticle.HeaderRunId = runId;
                break;
            case "subheader":
                editedArticle.SubheaderRunId = runId;
                break;
        }
    }

    private void UpdateEditedArticle(EditedArticle editedArticle, CollectionResult<ThreadMessage> messages)
    {
        foreach (var message in messages)
        {
            if (message.RunId == editedArticle.EditedBodyRunId)
            {
                editedArticle.EditedBody = message.Content.Last().Text;
                continue;
            }
            if (message.RunId == editedArticle.TranslatedBodyRunId)
            {
                editedArticle.TranslatedBody = message.Content.Last().Text;
                continue;
            }

            if (message.RunId == editedArticle.HeaderRunId)
            {
                editedArticle.Header = message.Content.Last().Text;
                continue;
            }

            if (message.RunId == editedArticle.SubheaderRunId)
            {
                editedArticle.Subheader = message.Content.Last().Text;
            }
        }
    }
}