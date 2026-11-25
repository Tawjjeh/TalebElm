using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class ExternalPlatformEnumTests
{
    [Fact]
    public void ExternalPlatform_HasStablePersistedValues()
    {
        Assert.Equal(2, Enum.GetValues<ExternalPlatform>().Length);
        Assert.Equal(1, (int)ExternalPlatform.Codeforces);
        Assert.Equal(2, (int)ExternalPlatform.GitHub);
    }
}