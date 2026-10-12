using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence;
using TalebElm.Tests.Infrastructure;
using Xunit;

public class TrackModuleRelationshipTests:SqliteTestBase
{
    [Fact]
    public void Module_ShouldHaveTrackForeignKey_WithRestrictDelete()
    {
        var moduleType = DbContext.Model.FindEntityType(typeof(Module));

        Assert.NotNull(moduleType);

        var foreignKey = Assert.Single(
            moduleType!.GetForeignKeys(),
            fk => fk.Properties.Any(
                p => p.Name == nameof(Module.TrackId))
                && fk.PrincipalEntityType.ClrType == typeof(Track));

        Assert.Equal(
            nameof(Module.TrackId),
            Assert.Single(foreignKey.Properties).Name);

        Assert.Equal(
            nameof(Track.Id),
            Assert.Single(foreignKey.PrincipalKey.Properties).Name);

        Assert.Equal(
            DeleteBehavior.Restrict,
            foreignKey.DeleteBehavior);
    }
}