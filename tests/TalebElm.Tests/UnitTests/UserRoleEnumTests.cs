using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class UserRoleEnumTests
{
    [Fact]
    public void UserRole_HasStablePersistedValues()
    {
        Assert.Equal(3, Enum.GetValues<UserRole>().Length);
        Assert.Equal(1, (int)UserRole.Student);
        Assert.Equal(2, (int)UserRole.Instructor);
        Assert.Equal(3, (int)UserRole.Admin);
    }
}