using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class ResourceStatusEnumTests
{
    [Fact]
    public void ResourceStatus_HasStablePersistedValues()
    {
        Assert.Equal(4, Enum.GetValues<ResourceStatus>().Length);
        Assert.Equal(1, (int)ResourceStatus.Approved);
        Assert.Equal(2, (int)ResourceStatus.PendingApproval);
        Assert.Equal(3, (int)ResourceStatus.Rejected);
        Assert.Equal(4, (int)ResourceStatus.Broken);
    }
}
