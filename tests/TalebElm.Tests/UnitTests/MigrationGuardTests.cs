using Microsoft.EntityFrameworkCore;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class MigrationGuardTests : SqliteTestBase
{
    [Fact]
    public void Model_HasNoPendingModelChanges()
    {
        Assert.False(DbContext.Database.HasPendingModelChanges());
    }
}
