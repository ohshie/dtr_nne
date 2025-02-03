using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Assistants;

namespace dtr_nne.Infrastructure.ExternalServices.LlmServices;

[ExcludeFromCodeCoverage]
[Experimental("OPENAI001")]
internal class OpenAiService(ILogger<OpenAiService> logger, 
    IOpenAiAssistantRepository repository, 
    ExternalService service) : IOpenAiService
{
    private readonly List<List<MessageContent>> _messages = [["translate"],["process"]];

    readonly List<string> _processingSteps = ["rewrite", "translate", "header", "subheader"];
    
    public async Task<ErrorOr<ArticleContent>> ProcessArticleAsync(ArticleContent article)
    {
        logger.LogInformation("Starting article processing article");
        var editedArticle = new EditedArticle();
        
        var client = new OpenAIClient(service.ApiKey);
        var assistantCollection = await repository.GetAll() as List<OpenAiAssistant>;
        if (assistantCollection is null)
        {
            return Errors.ExternalServiceProvider.Llm.InvalidAssistantRequested;
        }
        
        var assistantClient = client.GetAssistantClient();
        
        logger.LogDebug("Creating thread for article with body length: {BodyLength}", article.Body.Length);
        var thread = await CreateThread(assistantClient, article.Body);

        foreach (var step in _processingSteps)
        {
            logger.LogInformation("Processing step: {Step}", step);
            
            var assistant = await GetAssistantByPurpose(assistantClient, assistantCollection, step);
            if (assistant.IsError)
            {
                logger.LogError("Failed to get assistant for step {Step}: {Error}", step, assistant.FirstError.Code);
                return assistant.FirstError;
            }
            
            string runId;
            if (_processingSteps[0] == step)
            {
                logger.LogDebug("Running initial assistant step: {Step}", step);
                runId = await RunAssistantOnThreadAsync(thread.Id, 
                    assistantClient, 
                    assistant.Value, 
                    initialMessage: true);
            }
            else
            {
                runId = await RunAssistantOnThreadAsync(thread.Id, assistantClient, assistant.Value);
            }

            if (string.IsNullOrEmpty(runId))
            {
                logger.LogError("Assistant run failed for step {Step}", step);
                return Errors.ExternalServiceProvider.Llm.AssistantRunError;
            }
            
            logger.LogDebug("Writing run ID {RunId} for step {Step}", runId, step);
            WriteRunIdsToEditedArticle(editedArticle, step, runId);
        }
        
        logger.LogInformation("Retrieving messages from thread {ThreadId}", thread.Id);
        var messages = assistantClient.GetMessages(thread.Id,
            new MessageCollectionOptions { Order = MessageCollectionOrder.Descending });
        
        UpdateEditedArticle(editedArticle, messages);
        
        article.EditedArticle = editedArticle;
        logger.LogInformation("Article processing completed successfully");

        return article;
    }

    private async Task<AssistantThread> CreateThread(AssistantClient assistantClient, string articleBody)
    {
        var threadOpt = new ThreadCreationOptions
        {
            InitialMessages =
            {
                articleBody   
            }
        };

        var thread = (await assistantClient.CreateThreadAsync(threadOpt)).Value;
        logger.LogDebug("Thread created successfully with ID: {ThreadId}", thread.Id);
            
        return thread;
    }
    
    private async Task<ErrorOr<Assistant>> GetAssistantByPurpose(AssistantClient client,  List<OpenAiAssistant> assistantsCollection, string purpose)
    {
        var assistant = assistantsCollection.FirstOrDefault(assistant => assistant.Role == purpose);
        if (assistant is null)
        {
            logger.LogError("Invalid assistant purpose requested: {Purpose}", purpose);
            return Errors.ExternalServiceProvider.Llm.InvalidAssistantRequested;
        }
        
        logger.LogDebug("Retrieving assistant with ID: {AssistantId}", assistant.AssistantId);
        var clientResult = await client.GetAssistantAsync(assistant.AssistantId);
        return clientResult.Value;
    }

    private async Task<string> RunAssistantOnThreadAsync(string threadId, AssistantClient assistantClient, Assistant assistant, bool initialMessage = false)
    {
        if (!initialMessage)
        {
            logger.LogDebug("Creating user message in thread {ThreadId}", threadId);
            await assistantClient.CreateMessageAsync(threadId, MessageRole.User, _messages[0]);
        }
      
        logger.LogDebug("Creating run for assistant {AssistantId} in thread {ThreadId}", assistant.Id, threadId);
        var threadRun = await assistantClient.CreateRunAsync(threadId, assistant.Id);

        try
        {
            var completion = await PollForRunCompletionAsync(threadId, threadRun.Value.Id, assistantClient);
            logger.LogDebug("Run completed successfully with ID: {RunId}", completion); 
            return completion;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to run assistant on thread {ThreadId}", threadId);
            return string.Empty;
        }
    }

    private async Task<string> PollForRunCompletionAsync(string threadId, string runId, AssistantClient assistantClient, int maxAttempts = 100, int delayMilliSeconds = 500)
    {
        logger.LogDebug("Starting to poll for run completion. ThreadId: {ThreadId}, RunId: {RunId}", threadId, runId);
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            await Task.Delay(delayMilliSeconds);
            var threadRun = await assistantClient.GetRunAsync(threadId, runId);

            if (threadRun.Value.Status.IsTerminal)
            {
                logger.LogDebug("Run completed after {Attempts} attempts", attempt + 1);
                return runId;
            }
        }
        
        logger.LogWarning("Run polling timed out after {MaxAttempts} attempts", maxAttempts);
        throw new TimeoutException("Assistant run did not complete in time.");
    }
    
    private void WriteRunIdsToEditedArticle(EditedArticle editedArticle, string step, string runId)
    {
        logger.LogDebug("Writing run ID for step {Step}: {RunId}", step, runId);
        
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
        logger.LogDebug("Updating edited article with message content");
        
        foreach (var message in messages)
        {
            if (message.RunId == editedArticle.EditedBodyRunId)
            {
                editedArticle.EditedBody = message.Content[^1].Text;
                logger.LogDebug("Updated edited body from run {RunId}", message.RunId);
                continue;
            }
            if (message.RunId == editedArticle.TranslatedBodyRunId)
            {
                editedArticle.TranslatedBody = message.Content[^1].Text;
                logger.LogDebug("Updated translated body from run {RunId}", message.RunId);
                continue;
            }
            if (message.RunId == editedArticle.HeaderRunId)
            {
                editedArticle.Header = message.Content[^1].Text;
                logger.LogDebug("Updated header from run {RunId}", message.RunId);
                continue;
            }
            if (message.RunId == editedArticle.SubheaderRunId)
            {
                editedArticle.Subheader = message.Content[^1].Text;
                logger.LogDebug("Updated subheader from run {RunId}", message.RunId);
            }
        }
    }
}