using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class TechnologyKindEnumTests
{
    [Fact]
    public void TechnologyKind_HasStablePersistedValues()
    {
        Assert.Equal(4, Enum.GetValues<TechnologyKind>().Length);
        Assert.Equal(1, (int)TechnologyKind.Language);
        Assert.Equal(2, (int)TechnologyKind.Framework);
        Assert.Equal(3, (int)TechnologyKind.Topic);
        Assert.Equal(4, (int)TechnologyKind.Tool);
    }
}
