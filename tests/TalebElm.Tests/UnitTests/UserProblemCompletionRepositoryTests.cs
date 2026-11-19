using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class UserProblemCompletionRepositoryTests : SqliteTestBase
{
    private readonly UserProblemCompletionRepository _repository;

    public UserProblemCompletionRepositoryTests() => _repository = new UserProblemCompletionRepository(DbContext);

    private async Task<User> CreateUserAsync(string email)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Learner",
            Email = email,
            PasswordHash = "hash",
            JoinedAt = DateTime.UtcNow
        };

        DbContext.Users.Add(user);
        await SaveChangesAsync();
        return user;
    }

    private async Task<(Module First, Module Second)> CreateModulesAsync()
    {
        var track = new Track
        {
            Id = Guid.NewGuid(),
            Name = "Backend",
            Description = "Backend track"
        };

        DbContext.Tracks.Add(track);
        await SaveChangesAsync();

        var first = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "First", Order = 1 };
        var second = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "Second", Order = 2 };

        DbContext.Modules.AddRange(first, second);
        await SaveChangesAsync();

        return (first, second);
    }

    private async Task<ModuleProblem> AddProblemAsync(Guid moduleId, int order)
    {
        var problem = new ModuleProblem
        {
            Id = Guid.NewGuid(),
            ModuleId = moduleId,
            Title = $"Problem {order}",
            Kind = ProblemKind.CodeforcesProblem,
            ExternalIdentifier = $"{order}",
            Order = order,
            IsRequired = true
        };

        DbContext.Set<ModuleProblem>().Add(problem);
        await SaveChangesAsync();
        return problem;
    }

    private static UserProblemCompletion NewCompletion(Guid userId, Guid moduleProblemId)
        => new(userId, moduleProblemId, DateTime.UtcNow);

    [Fact]
    public async Task GetByUserAndProblemAsync_ReturnsMatchingCompletion()
    {
        var user = await CreateUserAsync("first@test.com");
        var (module, _) = await CreateModulesAsync();
        var problem = await AddProblemAsync(module.Id, 1);
        var completion = NewCompletion(user.Id, problem.Id);

        DbContext.UserProblemCompletions.Add(completion);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByUserAndProblemAsync(user.Id, problem.Id);

        Assert.NotNull(result);
        Assert.Equal(completion.Id, result.Id);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(problem.Id, result.ModuleProblemId);
    }

    [Fact]
    public async Task GetByUserAndProblemAsync_WhenCompletionIsMissing_ReturnsNull()
    {
        var user = await CreateUserAsync("second@test.com");
        var (module, _) = await CreateModulesAsync();
        var problem = await AddProblemAsync(module.Id, 1);

        var result = await _repository.GetByUserAndProblemAsync(user.Id, problem.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task CountForUserInModuleAsync_CountsOnlyTheSpecifiedUsersCompletionsInTheModule()
    {
        var firstUser = await CreateUserAsync("third@test.com");
        var secondUser = await CreateUserAsync("fourth@test.com");
        var (module, otherModule) = await CreateModulesAsync();
        var firstProblem = await AddProblemAsync(module.Id, 1);
        var secondProblem = await AddProblemAsync(module.Id, 2);
        var otherModuleProblem = await AddProblemAsync(otherModule.Id, 1);

        DbContext.UserProblemCompletions.AddRange(
            NewCompletion(firstUser.Id, firstProblem.Id),
            NewCompletion(firstUser.Id, secondProblem.Id),
            NewCompletion(firstUser.Id, otherModuleProblem.Id),
            NewCompletion(secondUser.Id, firstProblem.Id));
        await SaveChangesAsync();
        ClearTracker();

        var firstUserCount = await _repository.CountForUserInModuleAsync(firstUser.Id, module.Id);
        var secondUserCount = await _repository.CountForUserInModuleAsync(secondUser.Id, module.Id);

        Assert.Equal(2, firstUserCount);
        Assert.Equal(1, secondUserCount);
    }

    [Fact]
    public async Task CountForUserInModuleAsync_WhenNoCompletionsExist_ReturnsZero()
    {
        var user = await CreateUserAsync("fifth@test.com");
        var (module, _) = await CreateModulesAsync();

        var result = await _repository.CountForUserInModuleAsync(user.Id, module.Id);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task DuplicateCompletion_IsRejectedAndDoesNotInflateTheCount()
    {
        var user = await CreateUserAsync("sixth@test.com");
        var (module, _) = await CreateModulesAsync();
        var problem = await AddProblemAsync(module.Id, 1);

        await _repository.AddAsync(NewCompletion(user.Id, problem.Id));
        await SaveChangesAsync();

        await _repository.AddAsync(NewCompletion(user.Id, problem.Id));
        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
        ClearTracker();

        var count = await _repository.CountForUserInModuleAsync(user.Id, module.Id);

        Assert.Equal(1, count);
    }
}
