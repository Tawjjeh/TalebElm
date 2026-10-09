using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.Infrastructure;

/// <summary>
/// Base class for EF Core tests backed by a real relational engine: an
/// in-memory SQLite database (<c>DataSource=:memory:</c>).
/// <para>
/// A fresh database is created for every test class instance. xUnit creates
/// one instance per test method, so every test is isolated. The connection is
/// kept open for the lifetime of the fixture because an in-memory SQLite
/// database only exists while at least one connection to it is open. No local
/// <c>.db</c> file, external server, or mock is used, so the suite runs in CI.
/// </para>
/// <para>
/// <b>Usage pattern for dependent issues (#195, #199, #204, ...):</b>
/// </para>
/// <code>
/// public class YourRepositoryTests : SqliteTestBase
/// {
///     private readonly YourRepository _repository;
///
///     public YourRepositoryTests() => _repository = new YourRepository(DbContext);
///
///     [Fact]
///     public async Task GetByIdAsync_WhenExists_ReturnsEntity()
///     {
///         // Arrange
///         var entity = new YourEntity { /* ... */ };
///         await _repository.AddAsync(entity);
///         await SaveChangesAsync();
///         ClearTracker(); // force the next read to hit the database
///
///         // Act
///         var result = await _repository.GetByIdAsync(entity.Id);
///
///         // Assert
///         Assert.NotNull(result);
///     }
/// }
/// </code>
/// </summary>
public abstract class SqliteTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    private bool _disposed;

    protected AppDbContext DbContext { get; }

    protected SqliteTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
        _connection.Open();

        DbContext = CreateContext();
        DbContext.Database.EnsureCreated();
    }

    /// <summary>
    /// Creates another <see cref="AppDbContext"/> over the same open in-memory
    /// connection. Useful when a test needs a second, independent context to
    /// prove that data really reached the database.
    /// </summary>
    protected AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options);
    }

    protected Task SaveChangesAsync() => DbContext.SaveChangesAsync();

    protected void ClearTracker() => DbContext.ChangeTracker.Clear();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        DbContext.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
