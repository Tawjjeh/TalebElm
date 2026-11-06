using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;
public class ProblemKindTests
{
    [Fact]
    public void ProblemKind_HasExpectedValues()
    {
        Assert.Equal(1, (int)ProblemKind.CodeforcesProblem);
        Assert.Equal(2, (int)ProblemKind.GitHubTask);
    }
}
