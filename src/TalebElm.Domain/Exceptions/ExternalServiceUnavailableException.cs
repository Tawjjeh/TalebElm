using System;

namespace TalebElm.Domain.Exceptions;

/// <summary>
/// Raised when an external service (for example Codeforces or GitHub) cannot be reached, is rate
/// limited, times out, or returns a response that cannot be trusted.
/// <para>
/// This is deliberately distinct from "not found". A learner must never fail verification because
/// the external service is temporarily unavailable, so callers should surface this as a transient
/// failure (HTTP 503) and leave learner state unchanged.
/// </para>
/// </summary>
public class ExternalServiceUnavailableException : DomainException
{
    public ExternalServiceUnavailableException(string message) : base(message)
    { }

    public ExternalServiceUnavailableException(string message, Exception innerException) : base(message, innerException)
    { }
}
