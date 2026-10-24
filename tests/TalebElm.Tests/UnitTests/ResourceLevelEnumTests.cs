using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class ResourceLevelEnumTests
{
    [Fact]
    public void ResourceLevel_HasStablePersistedValues()
    {
        Assert.Equal(3, Enum.GetValues<ResourceLevel>().Length);
        Assert.Equal(1, (int)ResourceLevel.Beginner);
        Assert.Equal(2, (int)ResourceLevel.Intermediate);
        Assert.Equal(3, (int)ResourceLevel.Advanced);
    }
}
