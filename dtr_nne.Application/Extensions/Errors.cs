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
            description: "Something went wrong when trying to add a value to database");
        
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
            public static Error NoSavedApiKeyFound => Error.NotFound(
                code: "ServiceManager.Internal.NoSavedApiKeyFound",
                description: "There is no saved api key in db currently");
            public static Error BadApiKey => Error.Validation(
                code: "ServiceManager.Api.BadApiKeyProvided",
                description: "Provided Api Key Did Not Pass a Check");
        }

        public static class Llm
        {
            public static Error InvalidAssistantRequested => Error.NotFound(
                code: "ExternalServiceProvider.Llm.InvalidAssistantRequested",
                description: "Requested llm assistant not found");
            public static Error AssistantRunError => Error.Failure(
                code: "ExternalServiceProvider.Llm.AssistantRunError",
                description: "Something really wrong happened when trying to run Assistant on Thread");
        }
    }
    
    public static class NewsOutlets
    {
        public static Error NoNewsOutletProvided => Error.Validation(
            code: "NewsOutlet.NoNewsOutletProvided",
            description: "No suitable news outlet were provided");

        public static Error NotFoundInDb => Error.NotFound(
            "NewsOutlet.NotFound",
            description: "No NewsOutlets Were Found in Db");

        public static Error MatchFailed => Error.NotFound(
            "NewsOutlets.MatchFailed",
            description: "Provided NewsOutlets not found in Db");
        
        public static Error DeletionFailed => Error.Failure(
            code: "NewsOutlets.DeletionFailed",
            description: "Failed to delete one or more news outlets.");
        
        public static Error UpdateFailed => Error.Failure(
            code: "NewsOutlets.UpdateFailed",
            description: "Failed to update one or more news outlets.");
    }

    public static class Translator
    {
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