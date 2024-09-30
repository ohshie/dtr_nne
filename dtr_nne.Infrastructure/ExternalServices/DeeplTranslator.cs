using System.Runtime.CompilerServices;
using DeepL;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using ErrorOr;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Tests")]

namespace dtr_nne.Infrastructure.ExternalServices;

public class DeeplTranslator(ITranslatorApiRepository repository, ILogger<DeeplTranslator> logger) : ITranslatorService
{
    public async Task<ErrorOr<List<Headline>>> Translate(List<Headline> headlines, TranslatorApi? translatorApi = null)
    {
        logger.LogInformation("Starting translation process for {HeadlineCount} headlines", headlines.Count);
        
        if (headlines.Count < 1)
        {
            logger.LogWarning("No headlines provided for translation");
            return new List<Headline>();
        }

        translatorApi ??= await repository.Get(1);
        if (translatorApi is null || string.IsNullOrEmpty(translatorApi.ApiKey))
        {
            logger.LogError("No valid API key found");
            return Errors.Translator.Service.NoSavedApiKeyFound;
        }
        
        logger.LogDebug("Using API key from TranslatorApi with ID: {ApiKeyId}", translatorApi.ApiKey);
        
        var translatedHeadlines = await TranslateHeadlines(headlines, translatorApi.ApiKey);
        if (translatedHeadlines.IsError)
        {
            logger.LogError("Failed to process Headlines. {Error}", translatedHeadlines.FirstError.Description);
        }
        else
        {
            logger.LogInformation("Translation process completed for {HeadlineCount} headlines", headlines.Count);
        }
        
        return translatedHeadlines;
    }
    
    internal virtual async Task<ErrorOr<List<Headline>>> TranslateHeadlines(List<Headline> headlines, string apiKey)
    {
        logger.LogDebug("Starting to translate headlines with API key");
        
        var semaphore = new SemaphoreSlim(5);
        var tasks = new List<Task<ErrorOr<Headline>>>();
        
        foreach (var headline in headlines)
        {
            await semaphore.WaitAsync();
            
            var task = Task.Run<ErrorOr<Headline>>(async () =>
            {
                try
                {
                    if (string.IsNullOrEmpty(headline.OriginalHeadline))
                    {
                        logger.LogWarning("Empty original headline encountered");
                        headline.TranslatedHeadline = string.Empty;
                        return headline;
                    }
                    
                    logger.LogDebug("Translating headline: {OriginalHeadline}", headline.OriginalHeadline);
                    headline.TranslatedHeadline = await PerformRequest(headline.OriginalHeadline, apiKey);
                    logger.LogDebug("Translation successful for headline: {OriginalHeadline}", headline.OriginalHeadline);
                    return headline;
                }
                catch(Exception exception)
                {
                    if (exception is AuthorizationException)
                    {
                        logger.LogError("Invalid API key provided for translation");
                        headline.TranslatedHeadline = $"Provided ApiKey is not Valid!";
                        return Errors.Translator.Api.BadApiKey;
                    }
                    if (exception is QuotaExceededException)
                    {
                        logger.LogError("API quota exceeded during translation");
                        headline.TranslatedHeadline = $"Api Quota Exceeded!";
                        return Errors.Translator.Api.QuotaExceeded;
                    }
                    
                    logger.LogError(exception, "An unexpected error occurred during translation");
                    throw;
                }
                finally
                {
                    semaphore.Release();
                }
            });
            
            tasks.Add(task);
        }
        var results = await Task.WhenAll(tasks);
        
        var errors = results
            .Where(r => r.IsError)
            .Select(r => r.Errors.FirstOrDefault())
            .ToList();
        if (errors.Count != 0)
        {
            logger.LogError("Errors encountered during headline translation: {ErrorCount} errors", errors.Count);
            return errors.FirstOrDefault();
        }
        
        logger.LogDebug("All headlines translated successfully");
        return results.Select(r => r.Value).ToList();
    }

    internal virtual async Task<string> PerformRequest(string originalHeadline, string apiKey)
    {
        var translator = new Translator(apiKey, new TranslatorOptions{sendPlatformInfo = false});

        try
        {
            var translatedHeadline =
                (await translator.TranslateTextAsync(originalHeadline, LanguageCode.English, LanguageCode.Russian))
                .ToString();

            return translatedHeadline;
        }
        catch (Exception e)
        {
            logger.LogError("Error during translation request for headline: {OriginalHeadline}", originalHeadline);
            throw;
        }
    }
}