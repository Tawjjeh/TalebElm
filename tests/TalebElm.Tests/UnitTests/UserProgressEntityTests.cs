using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;

public class UserProgressEntityTests
{
    [Fact]
    public void NewUserProgress_IsLockedAndUnscored()
    {
        var progress = new UserProgress();

        Assert.False(progress.IsUnlocked);
        Assert.False(progress.PassedExam);
        Assert.Equal(0, progress.Score);
        Assert.Equal(Guid.Empty, progress.UserId);
        Assert.Equal(Guid.Empty, progress.ModuleId);
    }
}
