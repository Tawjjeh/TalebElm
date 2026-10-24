using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class ResourceKindEnumTests
{
    [Fact]
    public void ResourceKind_HasStablePersistedValues()
    {
        Assert.Equal(7, Enum.GetValues<ResourceKind>().Length);
        Assert.Equal(1, (int)ResourceKind.Book);
        Assert.Equal(2, (int)ResourceKind.Documentation);
        Assert.Equal(3, (int)ResourceKind.Video);
        Assert.Equal(4, (int)ResourceKind.Article);
        Assert.Equal(5, (int)ResourceKind.Repository);
        Assert.Equal(6, (int)ResourceKind.Course);
        Assert.Equal(7, (int)ResourceKind.Other);
    }
}
