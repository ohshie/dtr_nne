namespace dtr_nne.Application.Extensions;

public static class Errors
{
    public static class DbErrors
    {
        public static Error UnitOfWorkSaveFailed => Error.Failure(
            code: "DbErrors.UnitOfWorkSaveFailed",
            description: "Saving to Db Produced Error, refer to logs to get more info");
    }
    
    public static class NewsOutlets
    {
        public static Error NoNewsOutletProvided => Error.Validation(
            code: "NewsOutlet.NoNewsOutletProvided",
            description: "No suitable news outlet were provided");

        public static Error NotFoundInDb => Error.NotFound(
            "NewsOutlet.NotFound",
            description: "No NewsOutlets Were Found in Db");

        public static Error DeletionFailed => Error.Failure(
            code: "NewsOutlets.DeletionFailed",
            description: "Failed to delete one or more news outlets.");
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

        public static class Service
        {
            public static Error NoSavedApiKeyFound => Error.NotFound(
                code: "Translator.Service.NoSavedApiKeyFound",
                description: "There is no saved api key in db currently");
        }
    }
}