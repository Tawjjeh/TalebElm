using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence.Repositories;
using TalebElm.Tests.Infrastructure;


namespace TalebElm.Tests.UnitTests;

public class UserRepositoryTests : SqliteTestBase
{
    private readonly UserRepository _repository;

    public UserRepositoryTests() => _repository = new UserRepository(DbContext);

    private static User NewUser(string name) => new User
    {
        Name = name,
        Email = $"{name}@test.com",
        PasswordHash = "hash",
        JoinedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenExists()
    {
        var user = NewUser("ahmed");
        await _repository.AddAsync(user);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal("ahmed", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        await _repository.AddAsync(NewUser("a"));
        await _repository.AddAsync(NewUser("b"));
        await _repository.AddAsync(NewUser("c"));
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetAllAsync();

        Assert.Equal(3, result.Count);
        Assert.Equal(
            ["a@test.com", "b@test.com", "c@test.com"],
            result.Select(u => u.Email).OrderBy(e => e));
    }
    [Fact]
    public async Task GetAllAsync_DoesNotTrackEntities()
    {
        await _repository.AddAsync(NewUser("a"));
        await SaveChangesAsync();
        ClearTracker();

        await _repository.GetAllAsync();

        Assert.Empty(DbContext.ChangeTracker.Entries<User>());
    }
    [Fact]
    public async Task AddAsync_DoesNotPersist_UntilSave()
    {
        var user = NewUser("sara");

        await _repository.AddAsync(user);


        await using (var before = CreateContext())
            Assert.Equal(0, await before.Users.CountAsync());

        await SaveChangesAsync();

       
        await using (var after = CreateContext())
            Assert.Equal(1, await after.Users.CountAsync());
    }
}