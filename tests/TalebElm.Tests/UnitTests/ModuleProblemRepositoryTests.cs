using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class ModuleProblemRepositoryTests : SqliteTestBase
{
    private readonly ModuleProblemRepository _repository;

    public ModuleProblemRepositoryTests() => _repository = new ModuleProblemRepository(DbContext);

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

    private static ModuleProblem NewProblem(Guid moduleId, int order, bool isRequired, string? title = null)
        => new()
        {
            Id = Guid.NewGuid(),
            ModuleId = moduleId,
            Title = title ?? $"Problem {order}",
            Kind = ProblemKind.CodeforcesProblem,
            ExternalIdentifier = $"{order}",
            Order = order,
            IsRequired = isRequired
        };

    [Fact]
    public async Task GetByModuleAsync_ReturnsRequiredAndOptionalProblems()
    {
        var (module, _) = await CreateModulesAsync();
        var required = NewProblem(module.Id, 1, isRequired: true);
        var optional = NewProblem(module.Id, 2, isRequired: false);

        DbContext.Set<ModuleProblem>().AddRange(required, optional);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByModuleAsync(module.Id);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, problem => problem.Id == required.Id);
        Assert.Contains(result, problem => problem.Id == optional.Id);
    }

    [Fact]
    public async Task GetByModuleAsync_ReturnsProblemsOrderedByOrder()
    {
        var (module, _) = await CreateModulesAsync();
        var third = NewProblem(module.Id, 3, isRequired: true);
        var first = NewProblem(module.Id, 1, isRequired: true);
        var second = NewProblem(module.Id, 2, isRequired: false);

        DbContext.Set<ModuleProblem>().AddRange(third, first, second);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByModuleAsync(module.Id);

        Assert.Equal([1, 2, 3], result.Select(problem => problem.Order));
    }

    [Fact]
    public async Task GetByModuleAsync_ExcludesProblemsFromOtherModules()
    {
        var (module, otherModule) = await CreateModulesAsync();
        var included = NewProblem(module.Id, 1, isRequired: true);
        var excluded = NewProblem(otherModule.Id, 1, isRequired: true);

        DbContext.Set<ModuleProblem>().AddRange(included, excluded);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByModuleAsync(module.Id);

        Assert.Single(result);
        Assert.Equal(included.Id, result[0].Id);
    }

    [Fact]
    public async Task CountRequiredAsync_CountsOnlyRequiredProblems()
    {
        var (module, otherModule) = await CreateModulesAsync();
        DbContext.Set<ModuleProblem>().AddRange(
            NewProblem(module.Id, 1, isRequired: true),
            NewProblem(module.Id, 2, isRequired: true),
            NewProblem(module.Id, 3, isRequired: false),
            NewProblem(otherModule.Id, 1, isRequired: true));
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.CountRequiredAsync(module.Id);

        Assert.Equal(2, result);
    }

    [Fact]
    public async Task CountRequiredAsync_WhenModuleHasNoProblems_ReturnsZero()
    {
        var result = await _repository.CountRequiredAsync(Guid.NewGuid());

        Assert.Equal(0, result);
    }
}
