using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.IntegrationTests;

public class UserExternalAccountIndexTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;

    public UserExternalAccountIndexTests()
    {
        // The in-memory DB lives only as long as this connection is open
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.Migrate(); // runs the real migrations, so the real indexes exist
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static UserExternalAccount NewAccount(
        string handle,
        AccountVerificationStatus status,
        string? externalNumericId = null) => new()
        {
            UserId = Guid.NewGuid(),
            Platform = default, // replace with a real ExternalPlatform member if you prefer
            Handle = handle,
            Status = status,
            ExternalNumericId = externalNumericId
        };

    [Fact]
    public void Two_pending_rows_with_same_handle_save_fine()
    {
        _db.AddRange(
            NewAccount("alice", AccountVerificationStatus.Pending),
            NewAccount("alice", AccountVerificationStatus.Pending));

        _db.SaveChanges(); // should not throw
    }

    [Fact]
    public void Pending_and_verified_rows_with_same_handle_save_fine()
    {
        _db.AddRange(
            NewAccount("alice", AccountVerificationStatus.Pending),
            NewAccount("alice", AccountVerificationStatus.Verified));

        _db.SaveChanges(); // should not throw
    }

    [Fact]
    public void Two_verified_rows_with_same_handle_throw()
    {
        _db.AddRange(
            NewAccount("alice", AccountVerificationStatus.Verified),
            NewAccount("alice", AccountVerificationStatus.Verified));

        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }

    [Fact]
    public void Two_rows_with_same_external_numeric_id_throw()
    {
        _db.AddRange(
            NewAccount("alice", AccountVerificationStatus.Pending, externalNumericId: "12345"),
            NewAccount("bob", AccountVerificationStatus.Pending, externalNumericId: "12345"));

        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }

    [Fact]
    public void Two_rows_with_null_external_numeric_id_save_fine()
    {
        _db.AddRange(
            NewAccount("alice", AccountVerificationStatus.Pending, externalNumericId: null),
            NewAccount("bob", AccountVerificationStatus.Pending, externalNumericId: null));

        _db.SaveChanges(); // should not throw
    }
}
