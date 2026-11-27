namespace TalebElm.Infrastructure.Services;

/// <summary>
/// Settings for the Codeforces API client, bound to the "Codeforces" configuration section.
/// </summary>
public class CodeforcesOptions
{
    public const string SectionName = "Codeforces";

    /// <summary>The API base URL, ending with a slash.</summary>
    public string BaseUrl { get; set; } = "https://codeforces.com/api/";

    /// <summary>The per-request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 10;
}
