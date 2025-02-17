namespace dtr_nne.Application.Extensions;

public static class Errors
{
    public static class DbErrors
    {
        public static Error UnitOfWorkSaveFailed => Error.Failure(
            code: "DbErrors.UnitOfWorkSaveFailed",
            description: "Saving to Db Produced Error, refer to logs to get more info");

        public static Error AddingToDbFailed => Error.Failure(
            code: "DbErrors.AddingToDbFailed",
            description: "Something went wrong when trying to entity to database");
        
        public static Error RemovingFailed => Error.Failure(
            code: "DbErrors.RemovingFailed",
            description: "Something went wrong when trying to delete entity from database");
        
        public static Error UpdatingDbFailed => Error.Failure(
            code: "DbErrors.UpdatingDbFailed",
            description: "Something went wrong when trying to update a value in database");
    }

    public static class ExternalServiceProvider
    {
        public static class Service
        {
            public static Error InvalidRequestedServiceType => Error.Validation(
                code: "ExternalServiceProvider.InvalidRequestedServiceType",
                description: "Requested non existent service type");
            public static Error NoSavedServiceFound => Error.NotFound(
                code: "ExternalServiceProvider.NoSavedServiceFound",
                description: "Requested non existent service");
            public static Error NoActiveServiceFound => Error.NotFound(
                code: "ExternalServiceProvider.NoActiveServiceFound",
                description:
                "No active service found on request, please double check that " +
                "at least one of external services of that type is set as inUse = true");
            public static Error NoSavedApiKeyFound => Error.NotFound(
                code: "ServiceManager.Internal.NoSavedApiKeyFound",
                description: "There is no saved api key in db currently");
            public static Error BadApiKey => Error.Validation(
                code: "ServiceManager.Api.BadApiKeyProvided",
                description: "Provided Api Key Did Not Pass a Check");
        }

        public static class Llm
        {
            public static class OpenAi
            {
                public static Error EmptyAssistantListProvided => Error.Validation(
                    code: "ExternalServiceProvider.Llm.OpenAi.EmptyAssistantListProvided",
                    description: "Provided open ai assistant dto list is empty");
            }
            
            public static Error NoAssitantsSaved => Error.NotFound(
                code: "ExternalServiceProvider.Llm.NoAssitantsSaved",
                description: "No lls assistants exists in db");
            public static Error InvalidAssistantRequested => Error.NotFound(
                code: "ExternalServiceProvider.Llm.InvalidAssistantRequested",
                description: "Requested llm assistant not found");
            public static Error AssistantRunError => Error.Failure(
                code: "ExternalServiceProvider.Llm.AssistantRunError",
                description: "Something really wrong happened when trying to run Assistant on Thread");
        }
        
        public static class Scraper
        {
            public static Error ScrapingRequestError(string info) => Error.Failure(
                code: "ExternalServiceProvider.Scrapers.ScrapingRequestError",
                description: $"While attempting to scrape uri service encountered: {info}");
        }
    }
    
    public static class NewsArticles
    {
        public static Error NoNewNewsArticles => Error.NotFound(
            code: "NewsArticles.NoNewNewsArticles",
            description: "No new news articles were found since last parse");

        public static Error JsonSerializationError(string info = "") => Error.Validation(
            code: "NewsArticles.JsonSerializationError",
            description: $"Encountered error while attempting to deserialize json with parse results: {info}");
    }
    public static class ManagedEntities
    {
        public static Error NoEntitiesProvided => Error.Validation(
            code: "ManagedEntities.NoEntitiesProvided",
            description: "No suitable entities were provided");
        
        public static Error DeletionFailed(Type entityType) => Error.Failure(
            code: "ManagedEntities.DeletionFailed",
            description: $"Failed to delete one or more {entityType.FullName}");
        
        public static Error UpdateFailed(Type entityType) => Error.Failure(
            code: "ManagedEntities.UpdateFailed",
            description: $"Failed to update one or more {entityType.FullName}");
        
        public static Error NotFoundInDb(Type entityType) => Error.NotFound(
            "ManagedEntities.NotFoundInDb",
            description: $"No {entityType.FullName} were found in Db");
        
        public static class NewsOutlets
        {
            public static Error NoNewsOutletProvided => Error.Validation(
                code: "NewsOutlet.NoNewsOutletProvided",
                description: "No suitable news outlet were provided");

            public static Error MatchFailed => Error.NotFound(
                "NewsOutlets.MatchFailed",
                description: "Provided NewsOutlets not found in Db");
        }
    }
    

    public static class Translator
    {
        public static class Service
        {
            public static Error NoHeadlineProvided => Error.NotFound(
                code: "Translator.Service.NoHeadlineProvided",
                description: "No headlines were provided for translation");

            public static Error UnexpectedErrorFromService(string error = "") => Error.Unexpected(
                code: "Translator.Service.UnexpectedErrorFromService",
                description: $"While translating service produced: {error}");
        }
        public static class Api
        {
            public static Error BadApiKey => Error.Validation(
                code: "Translator.Api.BadApiKeyProvided",
                description: "Provided Api Key Did Not Pass a Check");
            
            public static Error QuotaExceeded => Error.Validation(
                code: "Translator.Api.QuotaExceeded",
                description: "Provided ApiKey Quota Exceeded ");

            public static Error AddingFailed => Error.Failure(
                code: "Translator.Api.AddingFailed",
                description: "Failed to add a new key to Db");
            
            public static Error UpdatingFailed => Error.Failure(
                code: "Translator.Api.UpdatingFailed",
                description: "Failed to update Db with a new key");
        }
    }
}