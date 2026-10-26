using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.Infrastructure;

public class BlankDatabaseMigrationTests : IAsyncLifetime
{
    private readonly string _dbFilePath;
    private AppDbContext _context = null!;

    public BlankDatabaseMigrationTests()
    {
        _dbFilePath = Path.Combine(Path.GetTempPath(), $"talebelm_migration_chain_{Guid.NewGuid():N}.db");
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_dbFilePath}")
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();

        // Ensure SQLite connection pool releases the file before attempting deletion.
        SqliteConnection.ClearAllPools();

        try
        {
            if (File.Exists(_dbFilePath))
            {
                File.Delete(_dbFilePath);
            }
        }
        catch
        {
            // Best effort file cleanup in test environment.
        }
    }

    [Fact]
    public async Task MigrateAsync_FromBlankDatabase_CreatesAllExpectedTables()
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table';";

        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
        }

        Assert.Contains("Users", tables);
        Assert.Contains("Tracks", tables);
        Assert.Contains("Modules", tables);
        Assert.Contains("Lessons", tables);
        Assert.Contains("LessonResources", tables);
        Assert.Contains("Exams", tables);
        Assert.Contains("ExamQuestions", tables);
        Assert.Contains("ExamOptions", tables);
        Assert.Contains("TrackEnrollments", tables);
        Assert.Contains("UserProgresses", tables);
    }

    [Fact]
    public async Task MigrateAsync_FromBlankDatabase_CreatesExpectedUniqueIndexes()
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'index';";

        var indexes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                indexes.Add(reader.GetString(0));
            }
        }

        Assert.Contains("IX_UserProgresses_UserId_ModuleId", indexes);
        Assert.Contains("IX_TrackEnrollments_UserId_TrackId", indexes);
        Assert.Contains("IX_Users_Email", indexes);
        Assert.Contains("IX_Exams_ModuleId", indexes);
    }

    [Fact]
    public void MigrateAsync_FromBlankDatabase_HasNoPendingModelChanges()
    {
        Assert.False(_context.Database.HasPendingModelChanges());
    }
}
