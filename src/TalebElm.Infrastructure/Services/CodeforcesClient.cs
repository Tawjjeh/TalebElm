using System.Net;
using System.Text.Json;
using TalebElm.Application.DTOs.External;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Exceptions;

namespace TalebElm.Infrastructure.Services;

public class CodeforcesClient(HttpClient httpClient) : ICodeforcesClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<CodeforcesUser?> GetUserProfileAsync(string handle, CancellationToken cancellationToken = default)
    {
        CodeforcesRequestValidator.ValidateHandle(handle);

        var envelope = await GetAsync<List<CodeforcesUser>>(
            $"user.info?handles={Uri.EscapeDataString(handle)}", cancellationToken);

        if (envelope.Status == "OK")
            return envelope.Result?.FirstOrDefault();

        if (IsNotFound(envelope.Comment))
            return null;

        throw new ExternalServiceUnavailableException("Codeforces returned an unexpected response.");
    }

    public async Task<IReadOnlyList<CodeforcesSubmission>> GetUserSubmissionsAsync(
        string handle, int from, int count, CancellationToken cancellationToken = default)
    {
        CodeforcesRequestValidator.ValidateHandle(handle);
        CodeforcesRequestValidator.ValidateRange(from, count);

        var envelope = await GetAsync<List<CodeforcesSubmission>>(
            $"user.status?handle={Uri.EscapeDataString(handle)}&from={from}&count={count}", cancellationToken);

        if (envelope.Status == "OK")
            return envelope.Result ?? [];

        if (IsNotFound(envelope.Comment))
            return [];

        throw new ExternalServiceUnavailableException("Codeforces returned an unexpected response.");
    }

    private async Task<CodeforcesEnvelope<T>> GetAsync<T>(string relativeUrl, CancellationToken ct)
    {
        try
        {
            using var response = await httpClient.GetAsync(relativeUrl, ct);

            if (response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500)
                throw new ExternalServiceUnavailableException("Codeforces is temporarily unavailable.");

            var body = await response.Content.ReadAsStringAsync(ct);

            return JsonSerializer.Deserialize<CodeforcesEnvelope<T>>(body, JsonOptions)
                   ?? throw new ExternalServiceUnavailableException("Codeforces returned an empty response.");
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceUnavailableException("Codeforces returned a malformed response.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceUnavailableException("Codeforces could not be reached.", ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new ExternalServiceUnavailableException("The Codeforces request timed out.", ex);
        }
    }

    private static bool IsNotFound(string? comment)
        => comment is not null && comment.Contains("not found", StringComparison.OrdinalIgnoreCase);
}
