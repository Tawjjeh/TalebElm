using Microsoft.EntityFrameworkCore;
using TalebElm.Infrastructure.Persistence;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class DemoDataSeederTests : SqliteTestBase
{
    [Fact]
    public async Task SeedAsync_IsIdempotent_AcrossRepeatedRuns()
    {
        await DemoDataSeeder.SeedAsync(DbContext);
        await DemoDataSeeder.SeedAsync(DbContext);

        Assert.Equal(1, await DbContext.Tracks.CountAsync());
        Assert.Equal(3, await DbContext.Modules.CountAsync());
        Assert.Equal(6, await DbContext.Lessons.CountAsync());
        Assert.Equal(1, await DbContext.Exams.CountAsync());
        Assert.Equal(2, await DbContext.Technologies.CountAsync());
        Assert.Equal(2, await DbContext.Resources.CountAsync());
    }
}
