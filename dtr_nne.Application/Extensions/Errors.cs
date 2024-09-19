using ErrorOr;

namespace dtr_nne.Application.Extensions;

public static class Errors
{
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
}