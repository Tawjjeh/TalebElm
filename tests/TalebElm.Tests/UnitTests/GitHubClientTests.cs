using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Exceptions;
using TalebElm.Infrastructure;
using TalebElm.Infrastructure.Services;

namespace TalebElm.Tests.UnitTests;

public class GitHubClientTests
{
    private const string RepositoryJson = """
        {
          "id": 2001,
          "name": "hello",
          "full_name": "octocat/hello",
          "owner": { "id": 1001, "login": "octocat" },
          "fork": false,
          "default_branch": "main"
        }
        """;

    [Fact]
    public async Task GetRepositoryAsync_EscapesEveryUntrustedPathSegment()
    {
        var handler = new FakeHandler(_ => JsonResponse(HttpStatusCode.OK, RepositoryJson));
        var client = CreateClient(handler);

        await client.GetRepositoryAsync("own#er", "re/po");

        var request = Assert.Single(handler.Requests);
        Assert.Equal("/repos/own%23er/re%2Fpo", request.RequestUri!.AbsolutePath);
        Assert.Equal("https", request.RequestUri.Scheme);
        Assert.Equal("api.github.com", request.RequestUri.Host);
    }

    [Fact]
    public async Task GetRepositoryAsync_NotFound_ReturnsNull()
    {
        var client = CreateClient(new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)));

        var repository = await client.GetRepositoryAsync("octocat", "missing");

        Assert.Null(repository);
    }

    [Fact]
    public async Task GetRepositoryAsync_InvalidSegment_ThrowsValidationException()
    {
        var handler = new FakeHandler(_ => JsonResponse(HttpStatusCode.OK, RepositoryJson));
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<ValidationException>(
            () => client.GetRepositoryAsync("bad owner", "repo"));
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task GetRepositoryAsync_UnavailableStatus_ThrowsWithoutLeakingResponseOrToken(HttpStatusCode status)
    {
        const string token = "ghp_supersecret-token";
        const string rawBody = "raw-secret-error-body";
        var handler = new FakeHandler(_ => TextResponse(status, rawBody));
        var client = CreateClient(handler, token);

        var exception = await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
            () => client.GetRepositoryAsync("octocat", "hello"));

        Assert.DoesNotContain(token, exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(rawBody, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetRepositoryAsync_HttpRequestException_ThrowsUnavailable()
    {
        var handler = new FakeHandler(_ => throw new HttpRequestException("connection refused"));
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
            () => client.GetRepositoryAsync("octocat", "hello"));
    }

    [Fact]
    public async Task GetRepositoryAsync_Timeout_ThrowsUnavailable()
    {
        var handler = new FakeHandler(_ => throw new TaskCanceledException("timed out"));
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
            () => client.GetRepositoryAsync("octocat", "hello"));
    }

    [Fact]
    public async Task GetRepositoryAsync_MalformedJson_ThrowsUnavailable()
    {
        var handler = new FakeHandler(_ => JsonResponse(HttpStatusCode.OK, """{ "id": "not-a-number" }"""));
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
            () => client.GetRepositoryAsync("octocat", "hello"));
    }

    [Fact]
    public async Task GetRepositoryAsync_UsesUserAgentAndOptionalBearerToken()
    {
        const string token = "ghp_test-token";
        var withTokenHandler = new FakeHandler(_ => JsonResponse(HttpStatusCode.OK, RepositoryJson));
        var withTokenClient = CreateClient(withTokenHandler, token);

        await withTokenClient.GetRepositoryAsync("octocat", "hello");

        var authorized = Assert.Single(withTokenHandler.Requests);
        Assert.Contains("TalebElm", authorized.Headers.UserAgent.ToString(), StringComparison.Ordinal);
        Assert.Equal("Bearer", authorized.Headers.Authorization?.Scheme);
        Assert.Equal(token, authorized.Headers.Authorization?.Parameter);

        var withoutTokenHandler = new FakeHandler(_ => JsonResponse(HttpStatusCode.OK, RepositoryJson));
        var withoutTokenClient = CreateClient(withoutTokenHandler);

        await withoutTokenClient.GetRepositoryAsync("octocat", "hello");

        var anonymous = Assert.Single(withoutTokenHandler.Requests);
        Assert.Null(anonymous.Headers.Authorization);
    }

    [Fact]
    public async Task GetRepositoryAsync_CallerCancellation_IsNotWrapped()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var handler = new FakeHandler(_ => JsonResponse(HttpStatusCode.OK, RepositoryJson));
        var client = CreateClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetRepositoryAsync("octocat", "hello", cts.Token));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task GetLatestWorkflowRunAsync_EscapesWorkflowAndBranchAndReturnsFirstRun()
    {
        const string workflowJson = """
            {
              "total_count": 2,
              "workflow_runs": [
                {
                  "id": 3001,
                  "name": "CI",
                  "head_sha": "a1b2c3d4e5",
                  "status": "completed",
                  "conclusion": "success",
                  "workflow_id": 99
                },
                {
                  "id": 3002,
                  "name": "CI",
                  "head_sha": "old",
                  "status": "completed",
                  "conclusion": "failure",
                  "workflow_id": 99
                }
              ]
            }
            """;
        var handler = new FakeHandler(_ => JsonResponse(HttpStatusCode.OK, workflowJson));
        var client = CreateClient(handler);

        var run = await client.GetLatestWorkflowRunAsync("octocat", "hello", "ci#test.yml", "feature/new");

        Assert.NotNull(run);
        Assert.Equal(3001L, run.Id);
        Assert.Equal("a1b2c3d4e5", run.HeadSha);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("/repos/octocat/hello/actions/workflows/ci%23test.yml/runs", request.RequestUri!.AbsolutePath);
        Assert.Equal("branch=feature%2Fnew&per_page=1", request.RequestUri.Query.TrimStart('?'));
    }

    [Fact]
    public async Task GetGitTreeAsync_UsesRecursiveQueryAndDeserializesSnakeCaseFields()
    {
        const string treeJson = """
            {
              "sha": "treesha456",
              "url": "https://api.github.com/repos/octocat/hello/git/trees/treesha456",
              "tree": [
                {
                  "path": "tests/UnitTest.cs",
                  "mode": "100644",
                  "type": "blob",
                  "sha": "blobsha123",
                  "size": 1024,
                  "url": "https://api.github.com/blob"
                }
              ],
              "truncated": true
            }
            """;
        var handler = new FakeHandler(_ => JsonResponse(HttpStatusCode.OK, treeJson));
        var client = CreateClient(handler);

        var tree = await client.GetGitTreeAsync("octocat", "hello", "treesha456", recursive: true);

        Assert.NotNull(tree);
        Assert.Equal("treesha456", tree.Sha);
        Assert.True(tree.Truncated);
        var item = Assert.Single(tree.Tree);
        Assert.Equal("tests/UnitTest.cs", item.Path);
        Assert.Equal(1024L, item.Size!.Value);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("/repos/octocat/hello/git/trees/treesha456", request.RequestUri!.AbsolutePath);
        Assert.Equal("?recursive=1", request.RequestUri.Query);
    }

    [Fact]
    public void AddInfrastructure_RegistersGitHubClient()
    {
        var configuration = BuildConfiguration();
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.IsType<GitHubClient>(provider.GetRequiredService<IGitHubClient>());
    }

    private static GitHubClient CreateClient(FakeHandler handler, string? token = null)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.github.com/"),
        };
        return new GitHubClient(httpClient, BuildConfiguration(token));
    }

    private static IConfiguration BuildConfiguration(string? token = null)
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:TalebElm"] = "Data Source=:memory:",
        };
        if (token is not null)
        {
            settings["GitHub:Token"] = token;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode status, string json) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static HttpResponseMessage TextResponse(HttpStatusCode status, string text) =>
        new(status) { Content = new StringContent(text, Encoding.UTF8, "text/plain") };

    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(responder(request));
        }
    }
}
