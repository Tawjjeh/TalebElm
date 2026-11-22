using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.IntegrationTests;


/// <summary>
/// Verifies the AddResourceIdFKToLessonResourceAndBackfillExistingRows migration:
/// seeds legacy LessonResources, applies the migration, and checks the backfill.
/// </summary>
public class ResourceBackfillMigrationTests : IAsyncLifetime
{
    private const string TargetMigrationSuffix = "AddResourceIdFKToLessonResourceAndBackfillExistingRows";

    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private AppDbContext _db = null!; // TODO: adjust to the name of your DbContext

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new AppDbContext(options);

        // Bring the schema to the migration just BEFORE the one under test.
        var migrations = _db.Database.GetMigrations().ToList();
        var targetIndex = migrations.FindIndex(m => m.EndsWith(TargetMigrationSuffix));
        Assert.True(targetIndex >= 0, $"Migration '{TargetMigrationSuffix}' was not found.");

        var previous = targetIndex == 0 ? "0" : migrations[targetIndex - 1];
        await _db.GetService<IMigrator>().MigrateAsync(previous);
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    // ---------- Seed data ----------

    private record Legacy(string Title, LessonResourceKind Kind, string? Url, string? Citation);

    private static readonly Legacy[] OneOfEachKind =
    [
        new("Clean Code",        LessonResourceKind.Book,          "https://example.com/book",    "Martin, 2008"),
        new("EF Core Docs",      LessonResourceKind.Documentation, "https://example.com/docs",    null),
        new("Intro Video",       LessonResourceKind.Video,         "https://example.com/video",   null),
        new("Great Article",     LessonResourceKind.Article,       "https://example.com/article", null),
        new("Sample Repo",       LessonResourceKind.Repository,    "https://example.com/repo",    null),
        new("Misc Resource",     LessonResourceKind.Other,         "https://example.com/other",   null),
    ];

    // Same Title/Kind/Url/Citation as the first row, used by a second lesson -> must dedupe.
    private static readonly Legacy Duplicate = OneOfEachKind[0];

    // Null Url and Citation must still be backfilled and linked.
    private static readonly Legacy NullUrlAndCitation =
        new("No Link Resource", LessonResourceKind.Article, null, null);

    private static string NewId() => Guid.NewGuid().ToString().ToUpperInvariant();

    // Seeds Track -> Module once, so lessons can reference a real Module.
    private async Task<string> SeedTrackAndModuleAsync()
    {
        var trackId = NewId();
        var moduleId = NewId();

        await ExecuteAsync(
            @"INSERT INTO ""Tracks"" (""Id"", ""CreatedAt"", ""Name"", ""Description"", ""Status"")
              VALUES ($id, CURRENT_TIMESTAMP, 'Test Track', 'Test track description', $status);",
            ("$id", trackId),
            ("$status", (int)TrackStatus.Draft));

        await ExecuteAsync(
            @"INSERT INTO ""Modules"" (""Id"", ""CreatedAt"", ""Title"", ""Summary"", ""Order"", ""TrackId"")
              VALUES ($id, CURRENT_TIMESTAMP, 'Test Module', 'Test module summary', 1, $trackId);",
            ("$id", moduleId),
            ("$trackId", trackId));

        return moduleId;
    }

    private async Task<string> SeedLessonAsync(string moduleId, int order)
    {
        var lessonId = NewId();

        await ExecuteAsync(
            @"INSERT INTO ""Lessons"" (""Id"", ""CreatedAt"", ""Title"", ""Content"", ""Order"", ""ModuleId"", ""Type"")
              VALUES ($id, CURRENT_TIMESTAMP, 'Test Lesson', 'Test lesson content', $order, $moduleId, $type);",
            ("$id", lessonId),
            ("$order", order),
            ("$moduleId", moduleId),
            ("$type", (int)LessonType.Text));

        return lessonId;
    }

    private async Task SeedLegacyRowsAsync(params Legacy[] rows)
    {
        var moduleId = await SeedTrackAndModuleAsync();
        var lessonOrder = 0;

        foreach (var row in rows)
        {
            // Each legacy row gets its own real Lesson, Order = 1 on the resource,
            // so the new unique (LessonId, Order) index is satisfied.
            var lessonId = await SeedLessonAsync(moduleId, ++lessonOrder);
            await ExecuteAsync(
                @"INSERT INTO ""LessonResources"" (""Id"", ""CreatedAt"", ""LessonId"", ""Order"", ""Title"", ""Kind"", ""Url"", ""Citation"")
                  VALUES ($id, CURRENT_TIMESTAMP, $lessonId, 1, $title, $kind, $url, $citation);",
                ("$id", NewId()),
                ("$lessonId", lessonId),
                ("$title", row.Title),
                ("$kind", (int)row.Kind),
                ("$url", row.Url),
                ("$citation", row.Citation));
        }
    }

    private async Task ApplyMigrationUnderTestAsync()
    {
        var target = _db.Database.GetMigrations().Single(m => m.EndsWith(TargetMigrationSuffix));
        await _db.GetService<IMigrator>().MigrateAsync(target);
    }

    // ---------- Tests ----------

    [Fact]
    public async Task Backfill_CreatesOneResourcePerDistinctLegacyRow_WithMatchingTitleAndUrl()
    {
        await SeedLegacyRowsAsync([.. OneOfEachKind, Duplicate, NullUrlAndCitation]);

        await ApplyMigrationUnderTestAsync();

        var resources = await ReadResourcesAsync();

        // 6 kinds + 1 null-url row; the duplicate must NOT create an extra Resource.
        Assert.Equal(OneOfEachKind.Length + 1, resources.Count);

        foreach (var expected in OneOfEachKind.Append(NullUrlAndCitation))
        {
            var match = Assert.Single(resources, r => r.Title == expected.Title);
            Assert.Equal(expected.Url, match.Url);
            Assert.Equal(expected.Citation, match.Citation);
        }
    }

    [Theory]
    [InlineData(LessonResourceKind.Book, ResourceKind.Book)]
    [InlineData(LessonResourceKind.Documentation, ResourceKind.Documentation)]
    [InlineData(LessonResourceKind.Video, ResourceKind.Video)]
    [InlineData(LessonResourceKind.Article, ResourceKind.Article)]
    [InlineData(LessonResourceKind.Repository, ResourceKind.Repository)]
    [InlineData(LessonResourceKind.Other, ResourceKind.Other)] // the case the reviewer called out
    public async Task Backfill_MapsLegacyKindToCorrectResourceKind(
        LessonResourceKind legacyKind, ResourceKind expectedKind)
    {
        await SeedLegacyRowsAsync(new Legacy($"Kind-{legacyKind}", legacyKind, "https://example.com/x", null));

        await ApplyMigrationUnderTestAsync();

        var resource = Assert.Single(await ReadResourcesAsync());
        Assert.Equal((int)expectedKind, resource.Kind);
    }

    [Fact]
    public async Task Backfill_PopulatesResourceId_AndDuplicatesShareTheSameResource()
    {
        await SeedLegacyRowsAsync([.. OneOfEachKind, Duplicate, NullUrlAndCitation]);

        await ApplyMigrationUnderTestAsync();

        var links = await ReadLinksAsync();

        // Every legacy row must now point at a Resource.
        Assert.Equal(OneOfEachKind.Length + 2, links.Count);
        Assert.All(links, l => Assert.False(string.IsNullOrEmpty(l.ResourceId)));

        // Linked Resource must match the legacy row it came from.
        var resources = (await ReadResourcesAsync()).ToDictionary(r => r.Id);
        Assert.All(links, l =>
        {
            var resource = resources[l.ResourceId!];
            Assert.Equal(l.Title, resource.Title);
            Assert.Equal(l.Url, resource.Url);
        });

        // The two "Clean Code" rows share ONE ResourceId.
        var cleanCodeIds = links.Where(l => l.Title == Duplicate.Title).Select(l => l.ResourceId).Distinct();
        Assert.Single(cleanCodeIds);
    }

    [Fact]
    public async Task Backfill_ResourceIdsAreUsableFromEfCore()
    {
        // Guards against the Guid text format written by raw SQL differing from what EF expects.
        await SeedLegacyRowsAsync(OneOfEachKind[0]);

        await ApplyMigrationUnderTestAsync();

        var link = Assert.Single(await ReadLinksAsync());
        var id = Guid.Parse(link.ResourceId!);

        var found = await _db.Set<TalebElm.Domain.Entities.Resource>().SingleOrDefaultAsync(r => r.Id == id);
        Assert.NotNull(found);
        Assert.Equal(OneOfEachKind[0].Title, found!.Title);
    }

    // ---------- Raw SQL helpers ----------

    private record ResourceRow(string Id, string Title, long Kind, string? Url, string? Citation);
    private record LinkRow(string Title, string? Url, string? ResourceId);

    private async Task<List<ResourceRow>> ReadResourcesAsync()
    {
        var list = new List<ResourceRow>();
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"SELECT ""Id"", ""Title"", ""Kind"", ""Url"", ""Citation"" FROM ""Resources"";";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new ResourceRow(
                reader.GetString(0), reader.GetString(1), reader.GetInt64(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4)));
        }
        return list;
    }

    private async Task<List<LinkRow>> ReadLinksAsync()
    {
        var list = new List<LinkRow>();
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"SELECT ""Title"", ""Url"", ""ResourceId"" FROM ""LessonResources"";";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new LinkRow(
                reader.GetString(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2)));
        }
        return list;
    }

    private async Task ExecuteAsync(string sql, params (string Name, object? Value)[] parameters)
    {
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }
}
