using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;

public class LessonEntityTests
{
    [Fact]
    public void NewLesson_HasEmptyContentAndNoOrder()
    {
        var lesson = new Lesson();

        Assert.Equal(string.Empty, lesson.Title);
        Assert.Equal(string.Empty, lesson.Content);
        Assert.Equal(0, lesson.Order);
        Assert.Equal(Guid.Empty, lesson.ModuleId);
    }
}
