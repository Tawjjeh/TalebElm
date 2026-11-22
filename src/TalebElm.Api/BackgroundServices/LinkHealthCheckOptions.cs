namespace TalebElm.Api.BackgroundServices;

public sealed class LinkHealthCheckOptions
{
    public const string SectionName = "LinkHealthCheck";

    public bool Enabled { get; set; } = true;
    public TimeSpan Interval { get; set; } = TimeSpan.FromDays(1);
}
