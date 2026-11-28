namespace TalebElm.Domain.Enums;

public enum VerificationFailureReason
{
    None = 0,                       // 200
    InvalidRepository = 1,          // 400
    JobNotFound = 2,                // 400
    ProblemNotFound = 3,            // 400
    TaskVersionRetired = 4,         // 400
    TemplateProvenanceMismatch = 5, // 400
    WorkflowNotSucceeded = 6,       // 400
    RepositoryNotOwnedByUser = 7,   // 403
    ProviderUnavailable = 8,        // 503
}
