using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class LessonResourceKindEnumTests
{
    [Fact]
    public void LessonResourceKind_HasStablePersistedValues()
    {
        Assert.Equal(6, Enum.GetValues<LessonResourceKind>().Length);
        Assert.Equal(1, (int)LessonResourceKind.Book);
        Assert.Equal(2, (int)LessonResourceKind.Documentation);
        Assert.Equal(3, (int)LessonResourceKind.Video);
        Assert.Equal(4, (int)LessonResourceKind.Article);
        Assert.Equal(5, (int)LessonResourceKind.Repository);
        Assert.Equal(6, (int)LessonResourceKind.Other);
    }
}
