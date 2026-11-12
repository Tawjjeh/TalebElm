using TalebElm.Application.DTOs.External;
using TalebElm.Domain.Exceptions;

namespace TalebElm.Application.Services.Abstraction;

/// <summary>
/// Read-only access to the public Codeforces API. Implementations live in Infrastructure and use a
/// typed <see cref="HttpClient"/>; business rules stay in the Application layer.
/// <para>
/// All parameters that come from a learner (<c>handle</c>) are untrusted. Implementations must
/// validate them (non-empty, length limits, allowed characters) and URL-escape them before building
/// a request. They must never concatenate raw input into a URL.
/// </para>
/// <para>
/// The methods distinguish "not found" from "temporarily unavailable". A not-found result is
/// <c>null</c> (or an empty list); an outage, rate limit, timeout or an unparseable response is an
/// <see cref="ExternalServiceUnavailableException"/>. A failure must never look like "handle not
/// found", otherwise a learner could fail verification during a Codeforces outage.
/// </para>
/// </summary>
public interface ICodeforcesClient
{
    /// <summary>
    /// Gets the public profile for a Codeforces handle.
    /// </summary>
    /// <param name="handle">The learner-supplied handle. Untrusted; implementations validate and escape it.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    /// The profile, or <c>null</c> when Codeforces positively reports that the handle does not exist.
    /// </returns>
    /// <exception cref="ExternalServiceUnavailableException">
    /// Codeforces is unreachable, rate limits the request, times out, or returns a malformed response.
    /// </exception>
    Task<CodeforcesUser?> GetUserProfileAsync(string handle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one page of submissions for a Codeforces handle from <c>user.status</c>, newest first.
    /// </summary>
    /// <param name="handle">The learner-supplied handle. Untrusted; implementations validate and escape it.</param>
    /// <param name="from">The 1-based index of the first submission to return. Must be at least 1.</param>
    /// <param name="count">
    /// The maximum number of submissions to return. Must be between 1 and 10000, which is Codeforces'
    /// per-request limit.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    /// The submissions in the requested range, or an empty list when the handle has no submissions in
    /// that range. Never <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method does not silently return "all" submissions. All-time solve verification must page
    /// with <paramref name="from"/> and <paramref name="count"/> until a page returns fewer than
    /// <paramref name="count"/> items, and must cap the number of pages so a runaway loop cannot
    /// hammer the API.
    /// </remarks>
    /// <exception cref="ExternalServiceUnavailableException">
    /// Codeforces is unreachable, rate limits the request, times out, or returns a malformed response.
    /// </exception>
    Task<IReadOnlyList<CodeforcesSubmission>> GetUserSubmissionsAsync(string handle, int from, int count, CancellationToken cancellationToken = default);
}
