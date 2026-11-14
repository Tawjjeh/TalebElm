using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class LessonTypeTests
{
    [Theory]
    [InlineData(LessonType.Text, 1)]
    [InlineData(LessonType.Video, 2)]
    [InlineData(LessonType.Exercise, 3)]
    public void LessonType_ShouldHaveExpectedIntegerValue(
         LessonType lessonType,
         int expectedValue)
    {
        Assert.Equal(expectedValue, (int)lessonType);
    }
}
