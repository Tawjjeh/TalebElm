using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TalebElm.Application.DTOs.External;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Exceptions;

namespace TalebElm.Infrastructure.Services;

public sealed class GitHubClient : IGitHubClient
{
    private const string DefaultBaseUrl = "https://api.github.com/";
    private const string UserAgent = "TalebElm";
    private const int MaxSegmentLength = 200;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;

    public GitHubClient(HttpClient httpClient, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(configuration);

        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri(DefaultBaseUrl, UriKind.Absolute);

        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        }

        string? token = configuration["GitHub:Token"];
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<GitHubRepository?> GetRepositoryAsync(
        string owner,
        string repo,
        CancellationToken cancellationToken = default)
    {
        string path = $"/repos/{Escape(owner, nameof(owner))}/{Escape(repo, nameof(repo))}";
        using HttpResponseMessage response = await SendAsync(path, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        GitHubRepository? repository = await ReadJsonAsync<GitHubRepository>(response, cancellationToken);
        if (repository is null || string.IsNullOrWhiteSpace(repository.FullName))
        {
            throw new ExternalServiceUnavailableException("GitHub returned an invalid repository response.");
        }

        return repository;
    }

    public async Task<GitHubWorkflowRun?> GetLatestWorkflowRunAsync(
        string owner,
        string repo,
        string workflow,
        string branch,
        CancellationToken cancellationToken = default)
    {
        string path =
            $"/repos/{Escape(owner, nameof(owner))}/{Escape(repo, nameof(repo))}" +
            $"/actions/workflows/{Escape(workflow, nameof(workflow))}/runs" +
            $"?branch={Escape(branch, nameof(branch))}&per_page=1";

        using HttpResponseMessage response = await SendAsync(path, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        WorkflowRunsEnvelope? envelope = await ReadJsonAsync<WorkflowRunsEnvelope>(response, cancellationToken);
        if (envelope?.WorkflowRuns is null)
        {
            throw new ExternalServiceUnavailableException("GitHub returned an invalid workflow-run response.");
        }

        return envelope.WorkflowRuns.FirstOrDefault();
    }

    public async Task<GitHubGitTree?> GetGitTreeAsync(
        string owner,
        string repo,
        string treeSha,
        bool recursive = true,
        CancellationToken cancellationToken = default)
    {
        string path =
            $"/repos/{Escape(owner, nameof(owner))}/{Escape(repo, nameof(repo))}" +
            $"/git/trees/{Escape(treeSha, nameof(treeSha))}?recursive={(recursive ? 1 : 0)}";

        using HttpResponseMessage response = await SendAsync(path, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        GitHubGitTree? tree = await ReadJsonAsync<GitHubGitTree>(response, cancellationToken);
        if (tree is null || tree.Tree is null || string.IsNullOrWhiteSpace(tree.Sha))
        {
            throw new ExternalServiceUnavailableException("GitHub returned an invalid Git tree response.");
        }

        return tree;
    }

    private async Task<HttpResponseMessage> SendAsync(string relativeUri, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                new Uri(relativeUri, UriKind.Relative),
                cancellationToken);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
            {
                return response;
            }

            int statusCode = (int)response.StatusCode;
            response.Dispose();
            throw new ExternalServiceUnavailableException(
                $"GitHub request failed with status code {statusCode}.");
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceUnavailableException("GitHub is unavailable.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ExternalServiceUnavailableException("GitHub request timed out.", ex);
        }
    }

    private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceUnavailableException("GitHub returned a malformed response.", ex);
        }
        catch (NotSupportedException ex)
        {
            throw new ExternalServiceUnavailableException("GitHub returned an unsupported response.", ex);
        }
    }

    private static string Escape(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > MaxSegmentLength
            || value.Any(char.IsControl)
            || value.Any(char.IsWhiteSpace))
        {
            throw new ValidationException(
                $"{parameterName} must be a non-empty value of at most {MaxSegmentLength} characters without whitespace or control characters.");
        }

        return Uri.EscapeDataString(value);
    }

    private sealed record WorkflowRunsEnvelope(IReadOnlyList<GitHubWorkflowRun> WorkflowRuns);
}
