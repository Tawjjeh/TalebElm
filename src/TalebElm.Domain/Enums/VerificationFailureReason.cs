namespace TalebElm.Domain.Enums;

public enum VerificationFailureReason
{
    None,                       // 200
    InvalidRepository,          // 400
    JobNotFound,                // 400
    ProblemNotFound,            // 400
    RepositoryNotOwnedByUser,   // 403
    ProviderUnavailable,        // 503
    // Add failure reasones that will be mapped with the status codes
}
