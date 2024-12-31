using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Assistants;

namespace dtr_nne.Infrastructure.ExternalServices.LlmServices;

[Experimental("OPENAI001")]
internal class OpenAiService : IOpenAiService
{
    private readonly ILogger<OpenAiService> _logger;
    private readonly IOpenAiAssistantRepository _repository;
    
    internal OpenAiService(ILogger<OpenAiService> logger, 
        IOpenAiAssistantRepository repository, ExternalService? service = null)
    {
        _logger = logger;
        _repository = repository;
        Service = service;
    }
    
    private readonly List<List<MessageContent>> _messages = [["translate"],["process"]];

    readonly List<string> _processingSteps = ["rewrite", "translate", "header", "subheader"];

    public ExternalService? Service { get; }

    public async Task<ErrorOr<Article>> ProcessArticleAsync(Article article, string apiKey = "")
    {
        _logger.LogInformation("Starting article processing article");
        var editedArticle = new EditedArticle();
        
        if (string.IsNullOrEmpty(apiKey))
        {
            if (Service is null || string.IsNullOrEmpty(Service.ApiKey))
            {
                return Errors.ExternalServiceProvider.Service.BadApiKey;
            }

            apiKey = Service.ApiKey;
        }
        
        var client = new OpenAIClient(apiKey);
        var assistantCollection = await _repository.GetAll() as List<OpenAiAssistant>;
        var assistantClient = client.GetAssistantClient();
        
        _logger.LogDebug("Creating thread for article with body length: {BodyLength}", article.Body.Length);
        var thread = await CreateThread(assistantClient, article.OriginalBody);

        foreach (var step in _processingSteps)
        {
            _logger.LogInformation("Processing step: {Step}", step);
            
            var assistant = await GetAssistantByPurpose(assistantClient, assistantCollection, step);
            if (assistant.IsError)
            {
                _logger.LogError("Failed to get assistant for step {Step}: {Error}", step, assistant.FirstError.Code);
                return assistant.FirstError;
            }
            
            string runId;
            if (_processingSteps[0] == step)
            {
                _logger.LogDebug("Running initial assistant step: {Step}", step);
                runId = await RunAssistantOnThreadAsync(thread.Value.Id, 
                    assistantClient, 
                    assistant.Value, 
                    initialMessage: true);
            }
            else
            {
                runId = await RunAssistantOnThreadAsync(thread.Value.Id, assistantClient, assistant.Value);
            }

            if (string.IsNullOrEmpty(runId))
            {
                _logger.LogError("Assistant run failed for step {Step}", step);
                return Errors.ExternalServiceProvider.Llm.AssistantRunError;
            }
            
            _logger.LogDebug("Writing run ID {RunId} for step {Step}", runId, step);
            WriteRunIdsToEditedArticle(editedArticle, step, runId);
        }
        
        _logger.LogInformation("Retrieving messages from thread {ThreadId}", thread.Value.Id);
        var messages = assistantClient.GetMessages(thread.Value.Id,
            new MessageCollectionOptions { Order = MessageCollectionOrder.Descending });
        
        UpdateEditedArticle(editedArticle, messages);
        
        article.EditedArticle = editedArticle;
        _logger.LogInformation("Article processing completed successfully");

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
        _logger.LogDebug("Thread created successfully with ID: {ThreadId}", thread.Id);
            
        return thread;
    }
    
    private async Task<ErrorOr<Assistant>> GetAssistantByPurpose(AssistantClient client,  List<OpenAiAssistant> assistantsCollection, string purpose)
    {
        var assistant = assistantsCollection.FirstOrDefault(assistant => assistant.Role == purpose);
        if (assistant is null)
        {
            _logger.LogError("Invalid assistant purpose requested: {Purpose}", purpose);
            return Errors.ExternalServiceProvider.Llm.InvalidAssistantRequested;
        }
        
        _logger.LogDebug("Retrieving assistant with ID: {AssistantId}", assistant.AssistantId);
        var clientResult = await client.GetAssistantAsync(assistant.AssistantId);
        return clientResult.Value;
    }

    private async Task<string> RunAssistantOnThreadAsync(string threadId, AssistantClient assistantClient, Assistant assistant, bool initialMessage = false)
    {
        if (!initialMessage)
        {
            _logger.LogDebug("Creating user message in thread {ThreadId}", threadId);
            await assistantClient.CreateMessageAsync(threadId, MessageRole.User, _messages[0]);
        }
      
        _logger.LogDebug("Creating run for assistant {AssistantId} in thread {ThreadId}", assistant.Id, threadId);
        var threadRun = await assistantClient.CreateRunAsync(threadId, assistant.Id);

        try
        {
            var completion = await PollForRunCompletionAsync(threadId, threadRun.Value.Id, assistantClient);
            _logger.LogDebug("Run completed successfully with ID: {RunId}", completion); 
            return completion;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to run assistant on thread {ThreadId}", threadId);
            return string.Empty;
        }
    }

    private async Task<string> PollForRunCompletionAsync(string threadId, string runId, AssistantClient assistantClient, int maxAttempts = 100, int delayMilliSeconds = 500)
    {
        _logger.LogDebug("Starting to poll for run completion. ThreadId: {ThreadId}, RunId: {RunId}", threadId, runId);
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            await Task.Delay(delayMilliSeconds);
            var threadRun = assistantClient.GetRun(threadId, runId);

            if (threadRun.Value.Status.IsTerminal)
            {
                _logger.LogDebug("Run completed after {Attempts} attempts", attempt + 1);
                return runId;
            }
        }
        
        _logger.LogWarning("Run polling timed out after {MaxAttempts} attempts", maxAttempts);
        throw new TimeoutException("Assistant run did not complete in time.");
    }
    
    private void WriteRunIdsToEditedArticle(EditedArticle editedArticle, string step, string runId)
    {
        _logger.LogDebug("Writing run ID for step {Step}: {RunId}", step, runId);
        
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
        _logger.LogDebug("Updating edited article with message content");
        
        foreach (var message in messages)
        {
            if (message.RunId == editedArticle.EditedBodyRunId)
            {
                editedArticle.EditedBody = message.Content.Last().Text;
                _logger.LogDebug("Updated edited body from run {RunId}", message.RunId);
                continue;
            }
            if (message.RunId == editedArticle.TranslatedBodyRunId)
            {
                editedArticle.TranslatedBody = message.Content.Last().Text;
                _logger.LogDebug("Updated translated body from run {RunId}", message.RunId);
                continue;
            }
            if (message.RunId == editedArticle.HeaderRunId)
            {
                editedArticle.Header = message.Content.Last().Text;
                _logger.LogDebug("Updated header from run {RunId}", message.RunId);
                continue;
            }
            if (message.RunId == editedArticle.SubheaderRunId)
            {
                editedArticle.Subheader = message.Content.Last().Text;
                _logger.LogDebug("Updated subheader from run {RunId}", message.RunId);
            }
        }
    }
}