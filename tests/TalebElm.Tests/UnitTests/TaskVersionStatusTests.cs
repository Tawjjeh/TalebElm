using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class TaskVersionStatusTests
{
    [Theory]
    [InlineData(TaskVersionStatus.Active, 1)]
    [InlineData(TaskVersionStatus.Deprecated, 2)]
    [InlineData(TaskVersionStatus.Retired, 3)]
    public void TaskVersionStatus_ShouldHaveExpectedIntegerValue(
        TaskVersionStatus status,
        int expectedValue)
    {
        Assert.Equal(expectedValue, (int)status);
    }
}
